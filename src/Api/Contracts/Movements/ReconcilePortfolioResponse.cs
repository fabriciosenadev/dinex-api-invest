namespace DinExApi.Api.Contracts.Movements;

public sealed record ReconcilePortfolioAssetResponse(
    string AssetSymbol,
    decimal ExpectedQuantity,
    decimal CurrentQuantity,
    decimal Difference,
    string Status,
    string Reason);

public sealed record ReconcilePortfolioResponse(
    int TotalAssets,
    int MatchedAssets,
    int DivergentAssets,
    IReadOnlyCollection<ReconcilePortfolioAssetResponse> Assets);
