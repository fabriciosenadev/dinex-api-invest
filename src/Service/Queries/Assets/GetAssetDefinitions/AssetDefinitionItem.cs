namespace DinExApi.Service;

public sealed record AssetDefinitionItem(
    Guid Id,
    string Symbol,
    AssetType Type,
    string? Name,
    string? Document,
    string? Country,
    string? Currency,
    string? Sector,
    string? Segment,
    string? ShareClass,
    string? CvmCode,
    string? FiiCategory,
    string? Administrator,
    string? Manager,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
