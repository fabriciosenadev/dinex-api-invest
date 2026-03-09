namespace DinExApi.Infra;

public sealed class InMemoryCorporateEventProcessor(InMemoryDataStore dataStore) : ICorporateEventProcessor
{
    public Task<int> ApplyAsync(CorporateEvent corporateEvent, CancellationToken cancellationToken = default)
    {
        var affected = dataStore.ApplyCorporateEvent(corporateEvent);
        return Task.FromResult(affected);
    }
}
