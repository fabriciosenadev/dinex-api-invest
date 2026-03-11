namespace DinExApi.Infra;

public sealed class InMemoryAssetDefinitionRepository(InMemoryDataStore dataStore) : IAssetDefinitionRepository
{
    public Task AddAsync(AssetDefinition assetDefinition, CancellationToken cancellationToken = default)
    {
        dataStore.AddAssetDefinition(assetDefinition);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(AssetDefinition assetDefinition, CancellationToken cancellationToken = default)
    {
        dataStore.UpdateAssetDefinition(assetDefinition);
        return Task.CompletedTask;
    }

    public Task<AssetDefinition?> GetByIdAsync(Guid userId, Guid assetDefinitionId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(dataStore.FindAssetDefinitionById(userId, assetDefinitionId));
    }

    public Task<AssetDefinition?> GetBySymbolAsync(Guid userId, string symbol, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(dataStore.FindAssetDefinitionBySymbol(userId, symbol));
    }

    public Task<IReadOnlyCollection<AssetDefinition>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(dataStore.SnapshotAssetDefinitions(userId));
    }

    public Task DeleteAsync(Guid userId, Guid assetDefinitionId, CancellationToken cancellationToken = default)
    {
        dataStore.DeleteAssetDefinitionById(userId, assetDefinitionId);
        return Task.CompletedTask;
    }
}
