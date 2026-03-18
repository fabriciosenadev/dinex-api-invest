namespace DinExApi.Service;

public sealed record UpsertAssetDefinitionCommand(
    Guid UserId,
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
    string? Notes) : ICommand<OperationResult<Guid>>;
