namespace DinExApi.Api.Contracts.Assets;

public sealed record UpsertAssetDefinitionRequest(
    string Symbol,
    string Type,
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
    string? Notes);
