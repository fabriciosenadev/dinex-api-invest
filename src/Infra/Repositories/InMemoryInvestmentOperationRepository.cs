
namespace DinExApi.Infra;

public sealed class InMemoryInvestmentOperationRepository(InMemoryDataStore dataStore) : IInvestmentOperationRepository
{
    public Task AddAsync(InvestmentOperation operation, CancellationToken cancellationToken = default)
    {
        dataStore.Add(operation);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<PortfolioPosition>> GetPortfolioPositionsAsync(CancellationToken cancellationToken = default)
    {
        var positions = dataStore
            .Snapshot()
            .OrderBy(x => x.OccurredAtUtc)
            .GroupBy(x => x.AssetSymbol)
            .Select(BuildPosition)
            .Where(x => x.Quantity > 0)
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<PortfolioPosition>>(positions);
    }

    private static PortfolioPosition BuildPosition(IGrouping<string, InvestmentOperation> operations)
    {
        decimal quantity = 0;
        decimal averagePrice = 0;
        var currency = operations.First().UnitPrice.Currency;

        foreach (var operation in operations)
        {
            if (operation.Type == OperationType.Buy)
            {
                var totalCost = (quantity * averagePrice) + (operation.Quantity * operation.UnitPrice.Amount);
                quantity += operation.Quantity;
                averagePrice = quantity == 0 ? 0 : totalCost / quantity;
                continue;
            }

            quantity -= operation.Quantity;
            if (quantity <= 0)
            {
                quantity = 0;
                averagePrice = 0;
            }
        }

        return new PortfolioPosition(operations.Key, quantity, averagePrice, currency);
    }
}
