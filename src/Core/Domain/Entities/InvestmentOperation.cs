
namespace DinExApi.Core;

public sealed class InvestmentOperation : Entity
{
    public InvestmentOperation(
        string assetSymbol,
        OperationType type,
        decimal quantity,
        Money unitPrice,
        DateTime occurredAtUtc)
    {
        AssetSymbol = assetSymbol?.Trim().ToUpperInvariant() ?? string.Empty;
        Type = type;
        Quantity = quantity;
        UnitPrice = unitPrice;
        OccurredAtUtc = DateTime.SpecifyKind(occurredAtUtc, DateTimeKind.Utc);
        CreatedAt = DateTime.UtcNow;

        AddNotifications(
            new Contract<Notification>()
                .Requires()
                .IsNotNullOrWhiteSpace(AssetSymbol, "InvestmentOperation.AssetSymbol", "Asset symbol is required.")
                .IsGreaterThan(Quantity, 0, "InvestmentOperation.Quantity", "Quantity must be greater than zero.")
                .IsGreaterThan(UnitPrice.Amount, 0, "InvestmentOperation.UnitPrice", "Unit price must be greater than zero."));
    }

    public string AssetSymbol { get; private set; }
    public OperationType Type { get; private set; }
    public decimal Quantity { get; private set; }
    public Money UnitPrice { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }
}
