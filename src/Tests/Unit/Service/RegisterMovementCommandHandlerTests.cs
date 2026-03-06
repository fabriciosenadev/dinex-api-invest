
namespace DinExApi.Tests;

public sealed class RegisterMovementCommandHandlerTests
{
    [Fact]
    public async Task Should_Register_Movement_And_Return_Portfolio_Position()
    {
        var store = new InMemoryDataStore();
        var repository = new InMemoryInvestmentOperationRepository(store);
        var unitOfWork = new InMemoryUnitOfWork();
        var commandHandler = new RegisterMovementCommandHandler(repository, unitOfWork);
        var queryHandler = new GetPortfolioPositionsQueryHandler(repository);

        var operationId = await commandHandler.HandleAsync(new RegisterMovementCommand(
            "PETR4",
            OperationType.Buy,
            10,
            32.50m,
            "BRL",
            DateTime.UtcNow));

        var portfolio = await queryHandler.HandleAsync(new GetPortfolioPositionsQuery());
        var position = portfolio.Data!.First();

        Assert.True(operationId.Succeeded);
        Assert.NotEqual(Guid.Empty, operationId.Data);
        Assert.True(portfolio.Succeeded);
        Assert.Single(portfolio.Data!);
        Assert.Equal("PETR4", position.AssetSymbol);
        Assert.Equal(10, position.Quantity);
    }
}
