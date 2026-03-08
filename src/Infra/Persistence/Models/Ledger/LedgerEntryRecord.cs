namespace DinExApi.Infra;

public sealed class LedgerEntryRecord
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? AssetSymbol { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitPriceAmount { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; }
    public string Source { get; set; } = string.Empty;
    public string? ReferenceId { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
