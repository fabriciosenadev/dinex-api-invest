namespace DinExApi.Core;

public interface ICorporateEventRepository
{
    Task AddAsync(CorporateEvent entry, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CorporateEvent>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
