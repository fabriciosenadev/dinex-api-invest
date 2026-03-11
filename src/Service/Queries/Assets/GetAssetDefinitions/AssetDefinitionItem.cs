namespace DinExApi.Service;

public sealed record AssetDefinitionItem(
    Guid Id,
    string Symbol,
    AssetType Type,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
