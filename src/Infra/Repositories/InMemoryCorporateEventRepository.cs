namespace DinExApi.Infra;

public sealed class InMemoryCorporateEventRepository(InMemoryDataStore dataStore) : ICorporateEventRepository
{
    public Task AddAsync(CorporateEvent entry, CancellationToken cancellationToken = default)
    {
        dataStore.AddCorporateEvent(entry);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(CorporateEvent entry, CancellationToken cancellationToken = default)
    {
        dataStore.UpdateCorporateEvent(entry);
        return Task.CompletedTask;
    }

    public Task<CorporateEvent?> GetByIdAsync(Guid userId, Guid eventId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(dataStore.FindCorporateEventById(userId, eventId));
    }

    public Task DeleteAsync(Guid userId, Guid eventId, CancellationToken cancellationToken = default)
    {
        dataStore.DeleteCorporateEventById(userId, eventId);
        return Task.CompletedTask;
    }

    public Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        dataStore.DeleteCorporateEventsByUserId(userId);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<CorporateEvent>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(dataStore.SnapshotCorporateEvents(userId));
    }
}
