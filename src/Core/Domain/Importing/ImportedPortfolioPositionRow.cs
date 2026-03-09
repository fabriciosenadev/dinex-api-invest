namespace DinExApi.Core;

public sealed record ImportedPortfolioPositionRow(
    string AssetSymbol,
    decimal Quantity,
    string Currency);
