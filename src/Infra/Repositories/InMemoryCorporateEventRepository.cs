namespace DinExApi.Infra;

public sealed class InMemoryCorporateEventRepository(InMemoryDataStore dataStore) : ICorporateEventRepository
{
    public Task AddAsync(CorporateEvent entry, CancellationToken cancellationToken = default)
    {
        dataStore.AddCorporateEvent(entry);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<CorporateEvent>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(dataStore.SnapshotCorporateEvents(userId));
    }
}
