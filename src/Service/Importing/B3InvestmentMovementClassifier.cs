namespace DinExApi.Service;

public sealed class B3InvestmentMovementClassifier : IInvestmentMovementClassifier
{
    public MovementClassificationResult Classify(ImportedSpreadsheetRow row)
    {
        var description = Normalize(row.EventDescription);
        var detail = Normalize(row.MovementDetail ?? string.Empty);
        var direction = Normalize(row.Direction ?? string.Empty);
        var composed = $"{description} {detail}";
        var normalizedAssetSymbol = NormalizeAssetSymbol(row.AssetSymbol);
        var signedAmount = row.GrossAmount ?? row.NetAmount ?? 0m;

        if (Contains(composed, "leilao de fracao")
            || Contains(composed, "cisao"))
        {
            return new MovementClassificationResult(null, normalizedAssetSymbol);
        }

        if (Contains(composed, "compra / venda")
            || Contains(composed, "compra/venda"))
        {
            if (Contains(direction, "credito") || Contains(direction, "entrada"))
            {
                return new MovementClassificationResult(OperationType.Buy, normalizedAssetSymbol);
            }

            if (Contains(direction, "debito") || Contains(direction, "saida"))
            {
                return new MovementClassificationResult(OperationType.Sell, normalizedAssetSymbol);
            }
        }

        if (Contains(composed, "fracao em ativos")
            || Contains(composed, "grupamento")
            || Contains(composed, "vencimento")
            || Contains(composed, "resgate")
            || Contains(composed, "amortizacao")
            || Contains(composed, "venda")
            || Contains(composed, "exercicio de venda"))
        {
            return new MovementClassificationResult(OperationType.Sell, normalizedAssetSymbol);
        }

        if (Contains(composed, "bonificacao")
            || Contains(composed, "desdobro")
            || Contains(composed, "incorporacao")
            || Contains(composed, "subscricao")
            || Contains(composed, "exercicio de compra")
            || Contains(composed, "compra"))
        {
            return new MovementClassificationResult(OperationType.Buy, normalizedAssetSymbol);
        }

        var hasPositionData = Contains(composed, "liquidacao")
            || (Contains(composed, "transferencia") && !Contains(composed, "transferido"))
            || (Contains(composed, "transfer") && !Contains(composed, "transferido"));
        if (hasPositionData)
        {
            if (Contains(direction, "entrada") || Contains(direction, "credito"))
            {
                return new MovementClassificationResult(OperationType.Buy, normalizedAssetSymbol);
            }

            if (Contains(direction, "saida") || Contains(direction, "debito"))
            {
                return new MovementClassificationResult(OperationType.Sell, normalizedAssetSymbol);
            }

            if (signedAmount < 0)
            {
                return new MovementClassificationResult(OperationType.Buy, normalizedAssetSymbol);
            }

            if (signedAmount > 0)
            {
                return new MovementClassificationResult(OperationType.Sell, normalizedAssetSymbol);
            }
        }

        return new MovementClassificationResult(null, normalizedAssetSymbol);
    }

    private static bool Contains(string value, string pattern)
        => value.Contains(pattern, StringComparison.Ordinal);

    private static string Normalize(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        return normalized
            .Replace("á", "a", StringComparison.Ordinal)
            .Replace("à", "a", StringComparison.Ordinal)
            .Replace("â", "a", StringComparison.Ordinal)
            .Replace("ã", "a", StringComparison.Ordinal)
            .Replace("é", "e", StringComparison.Ordinal)
            .Replace("ê", "e", StringComparison.Ordinal)
            .Replace("í", "i", StringComparison.Ordinal)
            .Replace("ó", "o", StringComparison.Ordinal)
            .Replace("ô", "o", StringComparison.Ordinal)
            .Replace("õ", "o", StringComparison.Ordinal)
            .Replace("ú", "u", StringComparison.Ordinal)
            .Replace("ç", "c", StringComparison.Ordinal);
    }

    private static string? NormalizeAssetSymbol(string? assetSymbol)
        => string.IsNullOrWhiteSpace(assetSymbol)
            ? null
            : assetSymbol.Trim().ToUpperInvariant();
}
