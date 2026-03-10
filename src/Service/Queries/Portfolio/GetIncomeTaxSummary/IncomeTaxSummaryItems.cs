namespace DinExApi.Service;

public sealed record IncomeTaxAssetSummaryItem(
    string AssetSymbol,
    decimal Quantity,
    decimal AveragePrice,
    decimal TotalCost,
    string Currency);

public sealed record IncomeTaxCompanySummaryItem(
    string CompanyCode,
    decimal TotalQuantity,
    decimal ConsolidatedAveragePrice,
    decimal TotalCost,
    string Currency,
    IReadOnlyCollection<IncomeTaxAssetSummaryItem> Assets);

public sealed record IncomeTaxYearSummaryItem(
    int Year,
    IReadOnlyCollection<IncomeTaxCompanySummaryItem> Companies);
