namespace DinExApi.Infra;

internal sealed class SqliteCorporateEventRepository(IRepository<CorporateEventRecord> repository) : ICorporateEventRepository
{
    public async Task AddAsync(CorporateEvent entry, CancellationToken cancellationToken = default)
    {
        await repository.AddAsync(entry.ToRecord(), cancellationToken);
    }

    public Task UpdateAsync(CorporateEvent entry, CancellationToken cancellationToken = default)
    {
        repository.Update(entry.ToRecord());
        return Task.CompletedTask;
    }

    public async Task<CorporateEvent?> GetByIdAsync(Guid userId, Guid eventId, CancellationToken cancellationToken = default)
    {
        var record = await repository.Query()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == eventId, cancellationToken);

        return record?.ToEntity();
    }

    public async Task DeleteAsync(Guid userId, Guid eventId, CancellationToken cancellationToken = default)
    {
        var record = await repository.Query(false)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == eventId, cancellationToken);

        if (record is null)
        {
            return;
        }

        repository.Delete(record);
    }

    public async Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var records = await repository.Query(false)
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        if (records.Count == 0)
        {
            return;
        }

        repository.DeleteRange(records);
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
