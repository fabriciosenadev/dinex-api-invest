namespace DinExApi.Service;

public sealed record PortfolioPositionItem(string AssetSymbol, decimal Quantity, decimal AveragePrice, string Currency);
