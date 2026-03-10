
namespace DinExApi.Infra;

internal sealed class SqliteInvestmentOperationRepository(IRepository<InvestmentOperationRecord> repository) : IInvestmentOperationRepository
{
    public async Task AddAsync(InvestmentOperation operation, CancellationToken cancellationToken = default)
    {
        await repository.AddAsync(operation.ToRecord(), cancellationToken);
    }

    public async Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var records = await repository.Query(false)
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        if (records.Count == 0)
        {
            return;
        }

        repository.DeleteRange(records);
    }

    public async Task<IReadOnlyCollection<PortfolioPosition>> GetPortfolioPositionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var operations = await repository.Query()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.OccurredAtUtc)
            .ToListAsync(cancellationToken);

        var positions = operations
            .GroupBy(x => x.AssetSymbol)
            .Select(BuildPosition)
            .Where(x => x.Quantity > 0 && !AssetSymbolRules.IsSubscriptionRight(x.AssetSymbol))
            .ToArray();

        return positions;
    }

    public async Task<IReadOnlyCollection<InvestmentOperationSnapshot>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var operations = await repository.Query()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.OccurredAtUtc)
            .ThenBy(x => x.Id)
            .Select(x => new InvestmentOperationSnapshot(
                x.AssetSymbol,
                (OperationType)x.Type,
                x.Quantity,
                x.UnitPriceAmount,
                x.Currency,
                x.OccurredAtUtc))
            .ToArrayAsync(cancellationToken);

        return operations;
    }

    private static PortfolioPosition BuildPosition(IGrouping<string, InvestmentOperationRecord> operations)
    {
        decimal quantity = 0;
        decimal averagePrice = 0;
        var currency = operations.First().Currency;

        foreach (var operation in operations)
        {
            var operationType = (OperationType)operation.Type;
            if (operationType == OperationType.Buy)
            {
                var totalCost = (quantity * averagePrice) + (operation.Quantity * operation.UnitPriceAmount);
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
