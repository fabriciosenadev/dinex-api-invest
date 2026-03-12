namespace DinExApi.Service;

public sealed record GetAssetDefinitionsQuery(
    Guid UserId,
    int Page = PaginationRequest.DefaultPage,
    int PageSize = PaginationRequest.DefaultPageSize) : IQuery<OperationResult<IReadOnlyCollection<AssetDefinitionItem>>>;
