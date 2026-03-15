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

public sealed record IncomeTaxRealizedAssetSummaryResponse(
    string AssetSymbol,
    decimal SoldQuantity,
    decimal GrossProceeds,
    decimal CostBasis,
    decimal RealizedResult,
    string Currency);

public sealed record IncomeTaxRealizedSummaryResponse(
    decimal TotalProfit,
    decimal TotalLoss,
    decimal NetResult,
    IReadOnlyCollection<IncomeTaxRealizedAssetSummaryResponse> Assets);

public sealed record IncomeTaxYearSummaryResponse(
    int Year,
    IReadOnlyCollection<IncomeTaxCompanySummaryResponse> Companies,
    IncomeTaxRealizedSummaryResponse Realized);
