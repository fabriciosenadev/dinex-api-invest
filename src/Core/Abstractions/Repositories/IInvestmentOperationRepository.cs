
namespace DinExApi.Core;

public interface IInvestmentOperationRepository
{
    Task AddAsync(InvestmentOperation operation, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PortfolioPosition>> GetPortfolioPositionsAsync(CancellationToken cancellationToken = default);
}
