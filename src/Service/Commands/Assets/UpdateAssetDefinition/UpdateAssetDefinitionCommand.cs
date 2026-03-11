namespace DinExApi.Service;

public sealed record UpdateAssetDefinitionCommand(
    Guid UserId,
    Guid AssetDefinitionId,
    string Symbol,
    AssetType Type,
    string? Notes) : ICommand<OperationResult<Guid>>;
