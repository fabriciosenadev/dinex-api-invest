namespace DinExApi.Core;

public sealed record PortfolioPosition(string AssetSymbol, decimal Quantity, decimal AveragePrice, string Currency);
