
namespace DinExApi.Core;

public interface IInvestmentOperationRepository
{
    Task AddAsync(InvestmentOperation operation, CancellationToken cancellationToken = default);
    Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InvestmentOperationSnapshot>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PortfolioPosition>> GetPortfolioPositionsAsync(Guid userId, CancellationToken cancellationToken = default);
    async Task<PagedResult<PortfolioPosition>> GetPortfolioPositionsPagedAsync(
        Guid userId,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        var items = await GetPortfolioPositionsAsync(userId, cancellationToken);
        return items.ToPagedResult(pagination);
    }
}
