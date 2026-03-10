namespace DinExApi.Tests;

public sealed class GetIncomeTaxSummaryQueryHandlerTests
{
    [Fact]
    public async Task Should_Group_By_Year_And_Company_Code()
    {
        var repository = new FakeInvestmentOperationRepository(
        [
            new InvestmentOperationSnapshot("TAEE4", OperationType.Buy, 10m, 10m, "BRL", new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc)),
            new InvestmentOperationSnapshot("TAEE11", OperationType.Buy, 5m, 20m, "BRL", new DateTime(2024, 2, 10, 0, 0, 0, DateTimeKind.Utc)),
            new InvestmentOperationSnapshot("TAEE4", OperationType.Buy, 2m, 12m, "BRL", new DateTime(2025, 1, 10, 0, 0, 0, DateTimeKind.Utc)),
            new InvestmentOperationSnapshot("TAEE11", OperationType.Sell, 1m, 0m, "BRL", new DateTime(2025, 2, 10, 0, 0, 0, DateTimeKind.Utc))
        ]);
        var handler = new GetIncomeTaxSummaryQueryHandler(repository);

        var result = await handler.HandleAsync(new GetIncomeTaxSummaryQuery(Guid.NewGuid()));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data!.Count);

        var summary2025 = result.Data.First(x => x.Year == 2025);
        var taeeCompany = summary2025.Companies.First(x => x.CompanyCode == "TAEE");
        Assert.Equal(16m, taeeCompany.TotalQuantity);
        Assert.Equal(2, taeeCompany.Assets.Count);
    }

    [Fact]
    public async Task Should_Return_Empty_When_User_Has_No_Operations()
    {
        var repository = new FakeInvestmentOperationRepository([]);
        var handler = new GetIncomeTaxSummaryQueryHandler(repository);

        var result = await handler.HandleAsync(new GetIncomeTaxSummaryQuery(Guid.NewGuid()));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data!);
    }

    private sealed class FakeInvestmentOperationRepository(IReadOnlyCollection<InvestmentOperationSnapshot> operations)
        : IInvestmentOperationRepository
    {
        public Task AddAsync(InvestmentOperation operation, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<IReadOnlyCollection<InvestmentOperationSnapshot>> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(operations);

        public Task<IReadOnlyCollection<PortfolioPosition>> GetPortfolioPositionsAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<PortfolioPosition>>([]);
    }
}
