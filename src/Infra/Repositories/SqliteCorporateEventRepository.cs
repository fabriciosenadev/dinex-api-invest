namespace DinExApi.Infra;

internal sealed class SqliteCorporateEventRepository(IRepository<CorporateEventRecord> repository) : ICorporateEventRepository
{
    public async Task AddAsync(CorporateEvent entry, CancellationToken cancellationToken = default)
    {
        await repository.AddAsync(entry.ToRecord(), cancellationToken);
    }

    public async Task<IReadOnlyCollection<CorporateEvent>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var records = await repository.Query()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.EffectiveAtUtc)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return records.Select(x => x.ToEntity()).ToArray();
    }
}
