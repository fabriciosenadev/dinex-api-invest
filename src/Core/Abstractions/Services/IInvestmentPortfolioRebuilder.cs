namespace DinExApi.Core;

public interface IInvestmentPortfolioRebuilder
{
    Task<int> RebuildAsync(Guid userId, CancellationToken cancellationToken = default);
}
