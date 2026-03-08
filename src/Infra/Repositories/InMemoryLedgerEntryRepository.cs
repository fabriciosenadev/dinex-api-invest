namespace DinExApi.Infra;

internal sealed class InMemoryLedgerEntryRepository(InMemoryDataStore dataStore) : ILedgerEntryRepository
{
    public Task AddAsync(LedgerEntry entry, CancellationToken cancellationToken = default)
    {
        dataStore.AddLedgerEntry(entry);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<LedgerEntry>> GetByUserIdAsync(
        Guid userId,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        CancellationToken cancellationToken = default)
    {
        var entries = dataStore.SnapshotLedgerEntries(userId);

        if (fromUtc.HasValue)
        {
            entries = entries.Where(x => x.OccurredAtUtc >= fromUtc.Value).ToArray();
        }

        if (toUtc.HasValue)
        {
            entries = entries.Where(x => x.OccurredAtUtc <= toUtc.Value).ToArray();
        }

        var ordered = entries
            .OrderByDescending(x => x.OccurredAtUtc)
            .ThenByDescending(x => x.CreatedAt)
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<LedgerEntry>>(ordered);
    }
}
