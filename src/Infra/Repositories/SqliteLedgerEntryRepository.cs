namespace DinExApi.Infra;

internal sealed class SqliteLedgerEntryRepository(IRepository<LedgerEntryRecord> repository) : ILedgerEntryRepository
{
    public async Task AddAsync(LedgerEntry entry, CancellationToken cancellationToken = default)
    {
        await repository.AddAsync(entry.ToRecord(), cancellationToken);
    }

    public async Task<IReadOnlyCollection<LedgerEntry>> GetByUserIdAsync(
        Guid userId,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        CancellationToken cancellationToken = default)
    {
        var query = repository.Query()
            .Where(x => x.UserId == userId);

        if (fromUtc.HasValue)
        {
            query = query.Where(x => x.OccurredAtUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(x => x.OccurredAtUtc <= toUtc.Value);
        }

        var records = await query
            .OrderByDescending(x => x.OccurredAtUtc)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return records.Select(x => x.ToEntity()).ToArray();
    }
}
