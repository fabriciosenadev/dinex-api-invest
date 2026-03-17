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

public sealed record IncomeTaxMonthlyBucketCarryResponse(
    string AssetClass,
    string TradeMode,
    decimal LossCarry);

public sealed record IncomeTaxMonthlyBucketSummaryResponse(
    string AssetClass,
    string TradeMode,
    decimal GrossResult,
    decimal LossCompensated,
    decimal TaxableBase,
    decimal TaxRate,
    decimal TaxDue,
    decimal IrrfMonth,
    decimal IrrfCompensated,
    decimal DarfGenerated);

public sealed record IncomeTaxMonthlySummaryResponse(
    int Year,
    int Month,
    decimal TotalTax,
    decimal TotalIrrfMonth,
    decimal TotalIrrfCompensated,
    decimal DarfDue,
    IReadOnlyCollection<IncomeTaxMonthlyBucketSummaryResponse> Buckets,
    IReadOnlyCollection<IncomeTaxMonthlyBucketCarryResponse> EndingLossCarryByBucket);

public sealed record IncomeTaxYearSummaryResponse(
    int Year,
    IReadOnlyCollection<IncomeTaxCompanySummaryResponse> Companies,
    IncomeTaxRealizedSummaryResponse Realized,
    IReadOnlyCollection<IncomeTaxMonthlySummaryResponse> MonthlyTaxation);
