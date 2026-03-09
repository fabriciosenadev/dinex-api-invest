namespace DinExApi.Service;

public sealed record ReconcilePortfolioAssetResult(
    string AssetSymbol,
    decimal ExpectedQuantity,
    decimal CurrentQuantity,
    decimal Difference,
    string Status,
    string Reason);

public sealed record ReconcilePortfolioResult(
    int TotalAssets,
    int MatchedAssets,
    int DivergentAssets,
    IReadOnlyCollection<ReconcilePortfolioAssetResult> Assets);
