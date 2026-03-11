namespace DinExApi.Infra;

public sealed class AssetDefinitionRecord
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public int Type { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
