namespace DinExApi.Infra;

public sealed class CorporateEventRecord
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int Type { get; set; }
    public string SourceAssetSymbol { get; set; } = string.Empty;
    public string? TargetAssetSymbol { get; set; }
    public decimal Factor { get; set; }
    public DateTime EffectiveAtUtc { get; set; }
    public string? Notes { get; set; }
    public DateTime AppliedAtUtc { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
