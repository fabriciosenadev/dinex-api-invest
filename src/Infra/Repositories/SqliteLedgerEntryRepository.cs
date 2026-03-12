namespace DinExApi.Infra;

internal sealed class SqliteLedgerEntryRepository(IRepository<LedgerEntryRecord> repository) : ILedgerEntryRepository
{
    public async Task AddAsync(LedgerEntry entry, CancellationToken cancellationToken = default)
    {
        await repository.AddAsync(entry.ToRecord(), cancellationToken);
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

    public async Task<PagedResult<LedgerEntry>> GetByUserIdPagedAsync(
        Guid userId,
        PaginationRequest pagination,
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
            .ToPagedResultAsync(pagination, cancellationToken);

        return new PagedResult<LedgerEntry>
        {
            Items = records.Items.Select(x => x.ToEntity()).ToArray(),
            TotalCount = records.TotalCount,
            Page = records.Page,
            PageSize = records.PageSize
        };
    }
}
