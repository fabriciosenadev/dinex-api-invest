namespace DinExApi.Infra;

internal sealed class SqliteAssetDefinitionRepository(IRepository<AssetDefinitionRecord> repository) : IAssetDefinitionRepository
{
    public async Task AddAsync(AssetDefinition assetDefinition, CancellationToken cancellationToken = default)
    {
        await repository.AddAsync(assetDefinition.ToRecord(), cancellationToken);
    }

    public Task UpdateAsync(AssetDefinition assetDefinition, CancellationToken cancellationToken = default)
    {
        repository.Update(assetDefinition.ToRecord());
        return Task.CompletedTask;
    }

    public async Task<AssetDefinition?> GetByIdAsync(Guid userId, Guid assetDefinitionId, CancellationToken cancellationToken = default)
    {
        var record = await repository.Query()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == assetDefinitionId, cancellationToken);

        return record?.ToEntity();
    }

    public async Task<AssetDefinition?> GetBySymbolAsync(Guid userId, string symbol, CancellationToken cancellationToken = default)
    {
        var normalizedSymbol = symbol.Trim().ToUpperInvariant();
        var record = await repository.Query()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Symbol == normalizedSymbol, cancellationToken);

        return record?.ToEntity();
    }

    public async Task<IReadOnlyCollection<AssetDefinition>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var records = await repository.Query()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Symbol)
            .ToListAsync(cancellationToken);

        return records.Select(x => x.ToEntity()).ToArray();
    }

    public async Task DeleteAsync(Guid userId, Guid assetDefinitionId, CancellationToken cancellationToken = default)
    {
        var record = await repository.Query(false)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == assetDefinitionId, cancellationToken);

        if (record is null)
        {
            return;
        }

        repository.Delete(record);
    }
}
