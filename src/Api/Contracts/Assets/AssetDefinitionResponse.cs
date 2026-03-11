namespace DinExApi.Api.Contracts.Assets;

public sealed record AssetDefinitionResponse(
    Guid Id,
    string Symbol,
    string Type,
    string? Notes,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
