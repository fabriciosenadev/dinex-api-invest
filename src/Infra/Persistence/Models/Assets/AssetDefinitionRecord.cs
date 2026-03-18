namespace DinExApi.Infra;

public sealed class AssetDefinitionRecord
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public int Type { get; set; }
    public string? Name { get; set; }
    public string? Document { get; set; }
    public string? Country { get; set; }
    public string? Currency { get; set; }
    public string? Sector { get; set; }
    public string? Segment { get; set; }
    public string? ShareClass { get; set; }
    public string? CvmCode { get; set; }
    public string? FiiCategory { get; set; }
    public string? Administrator { get; set; }
    public string? Manager { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
