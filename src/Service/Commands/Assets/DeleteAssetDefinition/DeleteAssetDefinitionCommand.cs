namespace DinExApi.Service;

public sealed record DeleteAssetDefinitionCommand(
    Guid UserId,
    Guid AssetDefinitionId) : ICommand<OperationResult>;
