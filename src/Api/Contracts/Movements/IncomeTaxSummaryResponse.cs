namespace DinExApi.Api.Contracts.Movements;

public sealed record IncomeTaxAssetSummaryResponse(
    string AssetSymbol,
    decimal Quantity,
    decimal AveragePrice,
    decimal TotalCost,
    string Currency);

public sealed record IncomeTaxCompanySummaryResponse(
    string CompanyCode,
    decimal TotalQuantity,
    decimal ConsolidatedAveragePrice,
    decimal TotalCost,
    string Currency,
    IReadOnlyCollection<IncomeTaxAssetSummaryResponse> Assets);

public sealed record IncomeTaxYearSummaryResponse(
    int Year,
    IReadOnlyCollection<IncomeTaxCompanySummaryResponse> Companies);
