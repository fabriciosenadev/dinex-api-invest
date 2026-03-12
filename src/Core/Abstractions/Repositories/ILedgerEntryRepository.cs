namespace DinExApi.Core;

public interface ILedgerEntryRepository
{
    Task AddAsync(LedgerEntry entry, CancellationToken cancellationToken = default);
    Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<LedgerEntry>> GetByUserIdAsync(
        Guid userId,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        CancellationToken cancellationToken = default);
    async Task<PagedResult<LedgerEntry>> GetByUserIdPagedAsync(
        Guid userId,
        PaginationRequest pagination,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        CancellationToken cancellationToken = default)
    {
        var items = await GetByUserIdAsync(userId, fromUtc, toUtc, cancellationToken);
        return items.ToPagedResult(pagination);
    }
}
