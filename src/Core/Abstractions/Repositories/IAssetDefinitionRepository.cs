namespace DinExApi.Core;

public interface IAssetDefinitionRepository
{
    Task AddAsync(AssetDefinition assetDefinition, CancellationToken cancellationToken = default);
    Task UpdateAsync(AssetDefinition assetDefinition, CancellationToken cancellationToken = default);
    Task<AssetDefinition?> GetByIdAsync(Guid userId, Guid assetDefinitionId, CancellationToken cancellationToken = default);
    Task<AssetDefinition?> GetBySymbolAsync(Guid userId, string symbol, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<AssetDefinition>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid userId, Guid assetDefinitionId, CancellationToken cancellationToken = default);
}
