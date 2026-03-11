namespace DinExApi.Service;

public sealed class B3InvestmentMovementClassifier : IInvestmentMovementClassifier
{
    public MovementClassificationResult Classify(ImportedSpreadsheetRow row)
    {
        var description = Normalize(row.EventDescription);
        var direction = Normalize(row.Direction ?? string.Empty);
        var normalizedAssetSymbol = NormalizeAssetSymbol(row.AssetSymbol);
        var isSettlementTransfer = Contains(description, "transferencia")
            && Contains(description, "liquidacao")
            && !Contains(description, "transferido");

        if (!isSettlementTransfer)
        {
            if (!IsFixedIncomeSymbol(normalizedAssetSymbol))
            {
                return new MovementClassificationResult(null, normalizedAssetSymbol);
            }

            if (Contains(description, "aplicacao") || Contains(description, "compra"))
            {
                return new MovementClassificationResult(OperationType.Buy, normalizedAssetSymbol);
            }

            if (Contains(description, "vencimento")
                || Contains(description, "resgate")
                || Contains(description, "amortizacao")
                || Contains(description, "venda"))
            {
                return new MovementClassificationResult(OperationType.Sell, normalizedAssetSymbol);
            }

            return new MovementClassificationResult(null, normalizedAssetSymbol);
        }

        if (Contains(direction, "credito") || Contains(direction, "entrada"))
        {
            return new MovementClassificationResult(OperationType.Buy, normalizedAssetSymbol);
        }

        if (Contains(direction, "debito") || Contains(direction, "saida"))
        {
            return new MovementClassificationResult(OperationType.Sell, normalizedAssetSymbol);
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

    private static bool IsFixedIncomeSymbol(string? normalizedAssetSymbol)
    {
        if (string.IsNullOrWhiteSpace(normalizedAssetSymbol))
        {
            return false;
        }

        return normalizedAssetSymbol.StartsWith("TESOURO", StringComparison.Ordinal)
            || normalizedAssetSymbol.StartsWith("CDB-", StringComparison.Ordinal)
            || normalizedAssetSymbol.StartsWith("LCI-", StringComparison.Ordinal)
            || normalizedAssetSymbol.StartsWith("LCA-", StringComparison.Ordinal)
            || normalizedAssetSymbol.StartsWith("RDB-", StringComparison.Ordinal)
            || normalizedAssetSymbol.StartsWith("CRI-", StringComparison.Ordinal)
            || normalizedAssetSymbol.StartsWith("CRA-", StringComparison.Ordinal);
    }
}
