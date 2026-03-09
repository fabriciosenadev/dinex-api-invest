namespace DinExApi.Core;

public interface ICorporateEventRepository
{
    Task AddAsync(CorporateEvent entry, CancellationToken cancellationToken = default);
    Task UpdateAsync(CorporateEvent entry, CancellationToken cancellationToken = default);
    Task<CorporateEvent?> GetByIdAsync(Guid userId, Guid eventId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid userId, Guid eventId, CancellationToken cancellationToken = default);
    Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CorporateEvent>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
