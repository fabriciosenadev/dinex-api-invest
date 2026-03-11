namespace DinExApi.Service;

public sealed record GetAssetDefinitionsQuery(Guid UserId) : IQuery<OperationResult<IReadOnlyCollection<AssetDefinitionItem>>>;
