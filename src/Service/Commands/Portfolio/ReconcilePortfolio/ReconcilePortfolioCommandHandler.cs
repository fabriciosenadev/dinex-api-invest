namespace DinExApi.Service;

public sealed class ReconcilePortfolioCommandHandler(
    IPortfolioPositionSpreadsheetParser parser,
    IInvestmentOperationRepository investmentOperationRepository,
    IAssetAliasResolver assetAliasResolver)
    : ICommandHandler<ReconcilePortfolioCommand, OperationResult<ReconcilePortfolioResult>>
{
    public async Task<OperationResult<ReconcilePortfolioResult>> HandleAsync(
        ReconcilePortfolioCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = new OperationResult<ReconcilePortfolioResult>();

        try
        {
            if (command.File.Content.Length == 0)
            {
                result.AddError("Spreadsheet file is empty.");
                return result;
            }

            await using var stream = new MemoryStream(command.File.Content);
            var imported = await parser.ParseAsync(stream, command.File.FileName, cancellationToken);
            if (imported.Count == 0)
            {
                result.AddError("No valid position rows were found in the spreadsheet.");
                return result;
            }

            var expectedByAsset = imported
                .Where(x => !AssetSymbolRules.IsSubscriptionRight(x.AssetSymbol))
                .GroupBy(x => GetReconciliationKey(x.AssetSymbol, assetAliasResolver), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Key, x => x.Sum(y => y.Quantity), StringComparer.OrdinalIgnoreCase);
            var expectedLabelByAsset = imported
                .Where(x => !AssetSymbolRules.IsSubscriptionRight(x.AssetSymbol))
                .GroupBy(x => GetReconciliationKey(x.AssetSymbol, assetAliasResolver), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Key, x => x.Select(y => y.AssetSymbol).First(), StringComparer.OrdinalIgnoreCase);

            var currentPositions = await investmentOperationRepository.GetPortfolioPositionsAsync(command.UserId, cancellationToken);
            var currentByAsset = currentPositions
                .Where(x => !AssetSymbolRules.IsSubscriptionRight(x.AssetSymbol))
                .GroupBy(x => GetReconciliationKey(x.AssetSymbol, assetAliasResolver), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Key, x => x.Sum(y => y.Quantity), StringComparer.OrdinalIgnoreCase);
            var currentLabelByAsset = currentPositions
                .Where(x => !AssetSymbolRules.IsSubscriptionRight(x.AssetSymbol))
                .GroupBy(x => GetReconciliationKey(x.AssetSymbol, assetAliasResolver), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Key, x => x.Select(y => y.AssetSymbol).First(), StringComparer.OrdinalIgnoreCase);

            var assets = expectedByAsset.Keys
                .Union(currentByAsset.Keys, StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .Select(asset =>
                {
                    var expected = expectedByAsset.GetValueOrDefault(asset, 0m);
                    var current = currentByAsset.GetValueOrDefault(asset, 0m);
                    var difference = Math.Round(current - expected, 6, MidpointRounding.AwayFromZero);
                    var isMatch = difference == 0m;
                    var displayAssetSymbol = currentLabelByAsset.GetValueOrDefault(asset)
                        ?? expectedLabelByAsset.GetValueOrDefault(asset)
                        ?? asset;
                    return new ReconcilePortfolioAssetResult(
                        AssetSymbol: displayAssetSymbol,
                        ExpectedQuantity: expected,
                        CurrentQuantity: current,
                        Difference: difference,
                        Status: isMatch ? "OK" : "DIVERGENTE",
                        Reason: BuildReason(expected, current, difference));
                })
                .ToArray();

            var matched = assets.Count(x => x.Status == "OK");
            result.SetData(new ReconcilePortfolioResult(
                TotalAssets: assets.Length,
                MatchedAssets: matched,
                DivergentAssets: assets.Length - matched,
                Assets: assets));

            return result;
        }
        catch (Exception)
        {
            result.SetAsInternalServerError();
            result.AddError("Unexpected error while reconciling portfolio.");
            return result;
        }
    }

    private static string BuildReason(decimal expected, decimal current, decimal difference)
    {
        if (difference == 0m)
        {
            return "Sem divergencia.";
        }

        if (expected > 0 && current == 0)
        {
            return "Ativo ausente na carteira atual.";
        }

        if (expected == 0 && current > 0)
        {
            return "Ativo nao encontrado no relatorio de posicao.";
        }

        if (Math.Abs(difference) < 1m)
        {
            return "Diferenca fracionaria: revisar bonificacao/fracao/leilao.";
        }

        return "Quantidade divergente entre carteira atual e relatorio.";
    }

    private static string ToCanonicalAssetKey(string assetSymbol)
    {
        if (string.IsNullOrWhiteSpace(assetSymbol))
        {
            return string.Empty;
        }

        var normalized = assetSymbol.Trim().ToUpperInvariant();

        if (normalized.StartsWith("LCI-", StringComparison.Ordinal)
            || normalized.StartsWith("LCA-", StringComparison.Ordinal))
        {
            return normalized[4..];
        }

        if (normalized.StartsWith("CDB-", StringComparison.Ordinal))
        {
            var suffix = normalized[4..];
            if (suffix.StartsWith("CDB", StringComparison.Ordinal))
            {
                return suffix;
            }

            return $"CDB{suffix}";
        }

        return normalized;
    }

    private static string GetReconciliationKey(string assetSymbol, IAssetAliasResolver aliasResolver)
    {
        var canonical = ToCanonicalAssetKey(assetSymbol);
        var aliased = aliasResolver.Resolve(canonical);
        return ToCanonicalAssetKey(aliased);
    }
}
