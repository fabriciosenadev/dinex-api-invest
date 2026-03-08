namespace DinExApi.Core;

public sealed class LedgerEntry : Entity
{
    public Guid UserId { get; private set; }
    public LedgerEntryType Type { get; private set; }
    public string Description { get; private set; }
    public string? AssetSymbol { get; private set; }
    public decimal? Quantity { get; private set; }
    public decimal? UnitPriceAmount { get; private set; }
    public decimal GrossAmount { get; private set; }
    public decimal NetAmount { get; private set; }
    public string Currency { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }
    public string Source { get; private set; }
    public string? ReferenceId { get; private set; }
    public string? Metadata { get; private set; }

    public LedgerEntry(
        Guid userId,
        LedgerEntryType type,
        string description,
        decimal grossAmount,
        decimal netAmount,
        string currency,
        DateTime occurredAtUtc,
        string source,
        string? assetSymbol = null,
        decimal? quantity = null,
        decimal? unitPriceAmount = null,
        string? referenceId = null,
        string? metadata = null,
        DateTime? createdAt = null,
        Guid? id = null)
    {
        Id = id ?? Guid.NewGuid();
        UserId = userId;
        Type = type;
        Description = description.Trim();
        AssetSymbol = string.IsNullOrWhiteSpace(assetSymbol) ? null : assetSymbol.Trim().ToUpperInvariant();
        Quantity = quantity;
        UnitPriceAmount = unitPriceAmount;
        GrossAmount = grossAmount;
        NetAmount = netAmount;
        Currency = currency.Trim().ToUpperInvariant();
        OccurredAtUtc = occurredAtUtc;
        Source = source.Trim().ToLowerInvariant();
        ReferenceId = string.IsNullOrWhiteSpace(referenceId) ? null : referenceId.Trim();
        Metadata = string.IsNullOrWhiteSpace(metadata) ? null : metadata.Trim();
        CreatedAt = createdAt ?? DateTime.UtcNow;
        UpdatedAt = null;
        DeletedAt = null;

        Validate();
    }

    private void Validate()
    {
        AddNotifications(
            new Contract<Notification>()
                .Requires()
                .IsNotEmpty(UserId, "LedgerEntry.UserId", "UserId is required.")
                .IsNotNullOrEmpty(Description, "LedgerEntry.Description", "Description is required.")
                .IsLowerThan(Description, 160, "LedgerEntry.Description", "Description must have up to 160 characters.")
                .IsNotNullOrEmpty(Currency, "LedgerEntry.Currency", "Currency is required.")
                .IsGreaterOrEqualsThan(Currency.Length, 3, "LedgerEntry.Currency", "Currency must have 3 characters.")
                .IsLowerOrEqualsThan(Currency.Length, 3, "LedgerEntry.Currency", "Currency must have 3 characters.")
                .IsNotNullOrEmpty(Source, "LedgerEntry.Source", "Source is required.")
                .IsGreaterOrEqualsThan(GrossAmount, 0, "LedgerEntry.GrossAmount", "Gross amount cannot be negative.")
                .IsGreaterOrEqualsThan(NetAmount, 0, "LedgerEntry.NetAmount", "Net amount cannot be negative.")
        );

        if (OccurredAtUtc == default)
        {
            AddNotification("LedgerEntry.OccurredAtUtc", "OccurredAtUtc is required.");
        }

        var requiresPosition = Type is LedgerEntryType.Buy or LedgerEntryType.Sell;
        if (requiresPosition)
        {
            if (string.IsNullOrWhiteSpace(AssetSymbol))
            {
                AddNotification("LedgerEntry.AssetSymbol", "AssetSymbol is required for buy and sell entries.");
            }

            if (!Quantity.HasValue || Quantity.Value <= 0)
            {
                AddNotification("LedgerEntry.Quantity", "Quantity must be greater than zero for buy and sell entries.");
            }

            if (!UnitPriceAmount.HasValue || UnitPriceAmount.Value <= 0)
            {
                AddNotification("LedgerEntry.UnitPriceAmount", "UnitPriceAmount must be greater than zero for buy and sell entries.");
            }
        }
    }
}
