
namespace DinExApi.Core;

public interface IInvestmentOperationRepository
{
    Task AddAsync(InvestmentOperation operation, CancellationToken cancellationToken = default);
    Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PortfolioPosition>> GetPortfolioPositionsAsync(Guid userId, CancellationToken cancellationToken = default);
}
