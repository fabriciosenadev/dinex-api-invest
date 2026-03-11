namespace DinExApi.Service;

public sealed record UpsertAssetDefinitionCommand(
    Guid UserId,
    string Symbol,
    AssetType Type,
    string? Notes) : ICommand<OperationResult<Guid>>;
