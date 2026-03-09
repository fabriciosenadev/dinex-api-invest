namespace DinExApi.Core;

public static class AssetSymbolRules
{
    public static bool IsSubscriptionRight(string? assetSymbol)
    {
        if (string.IsNullOrWhiteSpace(assetSymbol))
        {
            return false;
        }

        var normalized = assetSymbol.Trim().ToUpperInvariant();
        if (!normalized.EndsWith("12", StringComparison.Ordinal))
        {
            return false;
        }

        if (normalized.Length is < 6 or > 7)
        {
            return false;
        }

        var prefixLength = normalized.Length - 2;
        for (var index = 0; index < prefixLength; index++)
        {
            if (!char.IsLetter(normalized[index]))
            {
                return false;
            }
        }

        return true;
    }
}
