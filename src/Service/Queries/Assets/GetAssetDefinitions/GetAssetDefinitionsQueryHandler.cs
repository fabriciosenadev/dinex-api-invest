namespace DinExApi.Service;

public sealed class GetAssetDefinitionsQueryHandler(IAssetDefinitionRepository assetDefinitionRepository)
    : IQueryHandler<GetAssetDefinitionsQuery, OperationResult<IReadOnlyCollection<AssetDefinitionItem>>>
{
    public async Task<OperationResult<IReadOnlyCollection<AssetDefinitionItem>>> HandleAsync(
        GetAssetDefinitionsQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = new OperationResult<IReadOnlyCollection<AssetDefinitionItem>>();
        var items = await assetDefinitionRepository.GetByUserIdAsync(query.UserId, cancellationToken);

        result.SetData(items
            .OrderBy(x => x.Symbol)
            .Select(x => new AssetDefinitionItem(
                x.Id,
                x.Symbol,
                x.Type,
                x.Notes,
                x.CreatedAt,
                x.UpdatedAt))
            .ToArray());

        return result;
    }
}
