namespace DinExApi.Core;

public interface ILedgerEntryRepository
{
    Task AddAsync(LedgerEntry entry, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<LedgerEntry>> GetByUserIdAsync(
        Guid userId,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        CancellationToken cancellationToken = default);
}
