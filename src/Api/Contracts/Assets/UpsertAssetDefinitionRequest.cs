namespace DinExApi.Api.Contracts.Assets;

public sealed record UpsertAssetDefinitionRequest(
    string Symbol,
    string Type,
    string? Notes);
