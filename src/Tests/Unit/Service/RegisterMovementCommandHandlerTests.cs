
namespace DinExApi.Tests;

public sealed class RegisterMovementCommandHandlerTests
{
    [Fact]
    public async Task Should_Register_Movement_And_Return_Portfolio_Position()
    {
        var repository = new FakeInvestmentOperationRepository();
        var ledgerRepository = new FakeLedgerRepository();
        var unitOfWork = new SpyUnitOfWork();
        var userId = Guid.NewGuid();
        var commandHandler = new RegisterMovementCommandHandler(repository, ledgerRepository, unitOfWork);
        var queryHandler = new GetPortfolioPositionsQueryHandler(repository);

        var operationId = await commandHandler.HandleAsync(new RegisterMovementCommand(
            userId,
            "PETR4",
            OperationType.Buy,
            10,
            32.50m,
            "BRL",
            DateTime.UtcNow));

        var portfolio = await queryHandler.HandleAsync(new GetPortfolioPositionsQuery(userId));
        var position = portfolio.Data!.First();

        Assert.True(operationId.Succeeded);
        Assert.NotEqual(Guid.Empty, operationId.Data);
        Assert.True(portfolio.Succeeded);
        Assert.Single(portfolio.Data!);
        Assert.Equal("PETR4", position.AssetSymbol);
        Assert.Equal(10, position.Quantity);
    }

    private sealed class FakeInvestmentOperationRepository : IInvestmentOperationRepository
    {
        private readonly List<InvestmentOperation> _operations = [];

        public Task AddAsync(InvestmentOperation operation, CancellationToken cancellationToken = default)
        {
            _operations.Add(operation);
            return Task.CompletedTask;
        }

        public Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            _operations.RemoveAll(x => x.UserId == userId);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<InvestmentOperationSnapshot>> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<InvestmentOperationSnapshot>>([]);

        public Task<IReadOnlyCollection<PortfolioPosition>> GetPortfolioPositionsAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var positions = _operations
                .Where(x => x.UserId == userId)
                .GroupBy(x => new { x.AssetSymbol, x.UnitPrice.Currency })
                .Select(g =>
                {
                    var quantity = g.Sum(x => x.Type == OperationType.Buy ? x.Quantity : -x.Quantity);
                    var buyOperations = g.Where(x => x.Type == OperationType.Buy).ToArray();
                    var totalBuyQuantity = buyOperations.Sum(x => x.Quantity);
                    var averagePrice = totalBuyQuantity > 0
                        ? buyOperations.Sum(x => x.Quantity * x.UnitPrice.Amount) / totalBuyQuantity
                        : 0m;

                    return new PortfolioPosition(g.Key.AssetSymbol, quantity, averagePrice, g.Key.Currency);
                })
                .Where(x => x.Quantity > 0)
                .ToArray();

            return Task.FromResult<IReadOnlyCollection<PortfolioPosition>>(positions);
        }
    }

    private sealed class FakeLedgerRepository : ILedgerEntryRepository
    {
        public Task AddAsync(LedgerEntry entry, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<IReadOnlyCollection<LedgerEntry>> GetByUserIdAsync(
            Guid userId,
            DateTime? fromUtc = null,
            DateTime? toUtc = null,
            CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<LedgerEntry>>([]);
    }
}
