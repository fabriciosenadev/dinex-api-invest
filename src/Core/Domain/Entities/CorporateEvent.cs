namespace DinExApi.Core;

public sealed class CorporateEvent : Entity
{
    public Guid UserId { get; private set; }
    public CorporateEventType Type { get; private set; }
    public string SourceAssetSymbol { get; private set; }
    public string? TargetAssetSymbol { get; private set; }
    public decimal Factor { get; private set; }
    public decimal? CashPerSourceUnit { get; private set; }
    public DateTime EffectiveAtUtc { get; private set; }
    public string? Notes { get; private set; }
    public DateTime AppliedAtUtc { get; private set; }

    public CorporateEvent(
        Guid userId,
        CorporateEventType type,
        string sourceAssetSymbol,
        string? targetAssetSymbol,
        decimal factor,
        decimal? cashPerSourceUnit,
        DateTime effectiveAtUtc,
        string? notes,
        DateTime? createdAt = null,
        DateTime? appliedAtUtc = null,
        Guid? id = null)
    {
        Id = id ?? Guid.NewGuid();
        UserId = userId;
        Type = type;
        SourceAssetSymbol = sourceAssetSymbol.Trim().ToUpperInvariant();
        TargetAssetSymbol = string.IsNullOrWhiteSpace(targetAssetSymbol) ? null : targetAssetSymbol.Trim().ToUpperInvariant();
        Factor = factor;
        CashPerSourceUnit = cashPerSourceUnit;
        EffectiveAtUtc = effectiveAtUtc;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        CreatedAt = createdAt ?? DateTime.UtcNow;
        AppliedAtUtc = appliedAtUtc ?? DateTime.UtcNow;
        UpdatedAt = null;
        DeletedAt = null;

        Validate();
    }

    private void Validate()
    {
        AddNotifications(
            new Contract<Notification>()
                .Requires()
                .IsNotEmpty(UserId, "CorporateEvent.UserId", "UserId is required.")
                .IsNotNullOrWhiteSpace(SourceAssetSymbol, "CorporateEvent.SourceAssetSymbol", "Source asset symbol is required.")
                .IsLowerOrEqualsThan(SourceAssetSymbol, 20, "CorporateEvent.SourceAssetSymbol", "Source asset symbol must have up to 20 characters.")
                .IsGreaterThan(Factor, 0, "CorporateEvent.Factor", "Factor must be greater than zero."));

        if (EffectiveAtUtc == default)
        {
            AddNotification("CorporateEvent.EffectiveAtUtc", "Effective date is required.");
        }

        if (!string.IsNullOrWhiteSpace(TargetAssetSymbol) && TargetAssetSymbol.Length > 20)
        {
            AddNotification("CorporateEvent.TargetAssetSymbol", "Target asset symbol must have up to 20 characters.");
        }

        if (!string.IsNullOrWhiteSpace(Notes) && Notes.Length > 500)
        {
            AddNotification("CorporateEvent.Notes", "Notes must have up to 500 characters.");
        }

        if ((Type == CorporateEventType.TickerChange || Type == CorporateEventType.IncorporationWithCash) && string.IsNullOrWhiteSpace(TargetAssetSymbol))
        {
            AddNotification("CorporateEvent.TargetAssetSymbol", "Target asset symbol is required for this event type.");
        }

        if (CashPerSourceUnit is < 0)
        {
            AddNotification("CorporateEvent.CashPerSourceUnit", "Cash per source unit cannot be negative.");
        }
    }
}
