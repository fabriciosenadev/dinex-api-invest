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

public sealed record IncomeTaxRealizedAssetSummaryItem(
    string AssetSymbol,
    decimal SoldQuantity,
    decimal GrossProceeds,
    decimal CostBasis,
    decimal RealizedResult,
    string Currency);

public sealed record IncomeTaxRealizedSummaryItem(
    decimal TotalProfit,
    decimal TotalLoss,
    decimal NetResult,
    IReadOnlyCollection<IncomeTaxRealizedAssetSummaryItem> Assets);

public sealed record IncomeTaxMonthlyBucketCarryItem(
    string AssetClass,
    string TradeMode,
    decimal LossCarry);

public sealed record IncomeTaxMonthlyBucketSummaryItem(
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

public sealed record IncomeTaxMonthlySummaryItem(
    int Year,
    int Month,
    decimal TotalTax,
    decimal TotalIrrfMonth,
    decimal TotalIrrfCompensated,
    decimal DarfDue,
    IReadOnlyCollection<IncomeTaxMonthlyBucketSummaryItem> Buckets,
    IReadOnlyCollection<IncomeTaxMonthlyBucketCarryItem> EndingLossCarryByBucket);

public sealed record IncomeTaxYearSummaryItem(
    int Year,
    IReadOnlyCollection<IncomeTaxCompanySummaryItem> Companies,
    IncomeTaxRealizedSummaryItem Realized,
    IReadOnlyCollection<IncomeTaxMonthlySummaryItem> MonthlyTaxation);
