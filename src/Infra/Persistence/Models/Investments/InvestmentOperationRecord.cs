namespace DinExApi.Infra;

public sealed class InvestmentOperationRecord
{
    public Guid Id { get; init; }
    public string AssetSymbol { get; init; } = string.Empty;
    public int Type { get; init; }
    public decimal Quantity { get; init; }
    public decimal UnitPriceAmount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTime OccurredAtUtc { get; init; }
}
