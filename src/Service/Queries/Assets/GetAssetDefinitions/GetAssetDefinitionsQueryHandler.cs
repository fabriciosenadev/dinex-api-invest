namespace DinExApi.Service;

public sealed class GetAssetDefinitionsQueryHandler(IAssetDefinitionRepository assetDefinitionRepository)
    : IQueryHandler<GetAssetDefinitionsQuery, OperationResult<IReadOnlyCollection<AssetDefinitionItem>>>
{
    public async Task<OperationResult<IReadOnlyCollection<AssetDefinitionItem>>> HandleAsync(
        GetAssetDefinitionsQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = new OperationResult<IReadOnlyCollection<AssetDefinitionItem>>();
        var items = await assetDefinitionRepository.GetByUserIdPagedAsync(
            query.UserId,
            new PaginationRequest(query.Page, query.PageSize),
            cancellationToken);

        result.SetData(items.Items
            .OrderBy(x => x.Symbol)
            .Select(x => new AssetDefinitionItem(
                x.Id,
                x.Symbol,
                x.Type,
                x.Name,
                x.Document,
                x.Country,
                x.Currency,
                x.Sector,
                x.Segment,
                x.ShareClass,
                x.CvmCode,
                x.FiiCategory,
                x.Administrator,
                x.Manager,
                x.Notes,
                x.CreatedAt,
                x.UpdatedAt))
            .ToArray());

        return result;
    }
}
