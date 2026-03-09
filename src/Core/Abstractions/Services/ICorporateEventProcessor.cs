namespace DinExApi.Core;

public interface ICorporateEventProcessor
{
    Task<int> ApplyAsync(CorporateEvent corporateEvent, CancellationToken cancellationToken = default);
}
