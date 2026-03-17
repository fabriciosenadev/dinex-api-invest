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
        var ledgerRepository = new FakeLedgerEntryRepository([]);
        var handler = new GetIncomeTaxSummaryQueryHandler(repository, ledgerRepository);

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
    public async Task Should_Calculate_Yearly_Realized_Profit_And_Loss()
    {
        var repository = new FakeInvestmentOperationRepository(
        [
            new InvestmentOperationSnapshot("ITSA4", OperationType.Buy, 10m, 10m, "BRL", new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
            new InvestmentOperationSnapshot("ITSA4", OperationType.Sell, 4m, 15m, "BRL", new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc)),
            new InvestmentOperationSnapshot("ITSA4", OperationType.Sell, 2m, 8m, "BRL", new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc))
        ]);
        var ledgerRepository = new FakeLedgerEntryRepository([]);
        var handler = new GetIncomeTaxSummaryQueryHandler(repository, ledgerRepository);

        var result = await handler.HandleAsync(new GetIncomeTaxSummaryQuery(Guid.NewGuid()));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);

        var summary2025 = result.Data!.Single(x => x.Year == 2025);
        Assert.Equal(20m, summary2025.Realized.TotalProfit);
        Assert.Equal(4m, summary2025.Realized.TotalLoss);
        Assert.Equal(16m, summary2025.Realized.NetResult);

        var itsa = summary2025.Realized.Assets.Single(x => x.AssetSymbol == "ITSA4");
        Assert.Equal(6m, itsa.SoldQuantity);
        Assert.Equal(76m, itsa.GrossProceeds);
        Assert.Equal(60m, itsa.CostBasis);
        Assert.Equal(16m, itsa.RealizedResult);
    }

    [Fact]
    public async Task Should_Apply_Stock_Common_Exemption_When_Monthly_Proceeds_Are_Up_To_20K()
    {
        var repository = new FakeInvestmentOperationRepository(
        [
            new InvestmentOperationSnapshot("ITSA4", OperationType.Buy, 1000m, 10m, "BRL", new DateTime(2025, 1, 3, 0, 0, 0, DateTimeKind.Utc)),
            new InvestmentOperationSnapshot("ITSA4", OperationType.Sell, 1000m, 11m, "BRL", new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc))
        ]);
        var ledgerRepository = new FakeLedgerEntryRepository([]);
        var handler = new GetIncomeTaxSummaryQueryHandler(repository, ledgerRepository);

        var result = await handler.HandleAsync(new GetIncomeTaxSummaryQuery(Guid.NewGuid()));

        Assert.True(result.Succeeded);
        var january = result.Data!.Single(x => x.Year == 2025).MonthlyTaxation.Single(x => x.Month == 1);
        var bucket = january.Buckets.Single(x => x.AssetClass == "acao" && x.TradeMode == "common");

        Assert.Equal(1000m, bucket.GrossResult);
        Assert.Equal(0m, bucket.TaxableBase);
        Assert.Equal(0m, bucket.TaxDue);
        Assert.Equal(0m, january.DarfDue);
    }

    [Fact]
    public async Task Should_Classify_DayTrade_And_Apply_20_Percent_Rate()
    {
        var repository = new FakeInvestmentOperationRepository(
        [
            new InvestmentOperationSnapshot("PETR4", OperationType.Buy, 10m, 10m, "BRL", new DateTime(2025, 1, 10, 10, 0, 0, DateTimeKind.Utc)),
            new InvestmentOperationSnapshot("PETR4", OperationType.Sell, 6m, 12m, "BRL", new DateTime(2025, 1, 10, 16, 0, 0, DateTimeKind.Utc))
        ]);
        var ledgerRepository = new FakeLedgerEntryRepository([]);
        var handler = new GetIncomeTaxSummaryQueryHandler(repository, ledgerRepository);

        var result = await handler.HandleAsync(new GetIncomeTaxSummaryQuery(Guid.NewGuid()));

        Assert.True(result.Succeeded);
        var january = result.Data!.Single(x => x.Year == 2025).MonthlyTaxation.Single(x => x.Month == 1);
        var dayTradeBucket = january.Buckets.Single(x => x.AssetClass == "acao" && x.TradeMode == "daytrade");

        Assert.Equal(12m, dayTradeBucket.GrossResult);
        Assert.Equal(12m, dayTradeBucket.TaxableBase);
        Assert.Equal(0.20m, dayTradeBucket.TaxRate);
        Assert.Equal(2.4m, dayTradeBucket.TaxDue);
    }

    [Fact]
    public async Task Should_Compensate_Loss_From_Previous_Month_In_Same_Bucket()
    {
        var repository = new FakeInvestmentOperationRepository(
        [
            new InvestmentOperationSnapshot("ITSA4", OperationType.Buy, 3000m, 10m, "BRL", new DateTime(2025, 1, 5, 0, 0, 0, DateTimeKind.Utc)),
            new InvestmentOperationSnapshot("ITSA4", OperationType.Sell, 3000m, 9m, "BRL", new DateTime(2025, 1, 20, 0, 0, 0, DateTimeKind.Utc)),
            new InvestmentOperationSnapshot("ITSA4", OperationType.Buy, 3000m, 10m, "BRL", new DateTime(2025, 2, 3, 0, 0, 0, DateTimeKind.Utc)),
            new InvestmentOperationSnapshot("ITSA4", OperationType.Sell, 3000m, 12m, "BRL", new DateTime(2025, 2, 18, 0, 0, 0, DateTimeKind.Utc))
        ]);
        var ledgerRepository = new FakeLedgerEntryRepository([]);
        var handler = new GetIncomeTaxSummaryQueryHandler(repository, ledgerRepository);

        var result = await handler.HandleAsync(new GetIncomeTaxSummaryQuery(Guid.NewGuid()));

        Assert.True(result.Succeeded);
        var february = result.Data!.Single(x => x.Year == 2025).MonthlyTaxation.Single(x => x.Month == 2);
        var bucket = february.Buckets.Single(x => x.AssetClass == "acao" && x.TradeMode == "common");

        Assert.Equal(6000m, bucket.GrossResult);
        Assert.Equal(3000m, bucket.LossCompensated);
        Assert.Equal(3000m, bucket.TaxableBase);
        Assert.Equal(450m, bucket.TaxDue);
    }

    [Fact]
    public async Task Should_Apply_Irrf_To_Reduce_Darf()
    {
        var operations = new[]
        {
            new InvestmentOperationSnapshot("ITSA4", OperationType.Buy, 3000m, 10m, "BRL", new DateTime(2025, 3, 3, 0, 0, 0, DateTimeKind.Utc)),
            new InvestmentOperationSnapshot("ITSA4", OperationType.Sell, 3000m, 12m, "BRL", new DateTime(2025, 3, 20, 0, 0, 0, DateTimeKind.Utc))
        };

        var ledgerEntries = new[]
        {
            CreateTaxEntry("IRRF dedo-duro", 100m, new DateTime(2025, 3, 10, 0, 0, 0, DateTimeKind.Utc)),
            CreateTaxEntry("Imposto de renda retido", 50m, new DateTime(2025, 3, 25, 0, 0, 0, DateTimeKind.Utc))
        };

        var repository = new FakeInvestmentOperationRepository(operations);
        var ledgerRepository = new FakeLedgerEntryRepository(ledgerEntries);
        var handler = new GetIncomeTaxSummaryQueryHandler(repository, ledgerRepository);

        var result = await handler.HandleAsync(new GetIncomeTaxSummaryQuery(Guid.NewGuid()));

        Assert.True(result.Succeeded);
        var march = result.Data!.Single(x => x.Year == 2025).MonthlyTaxation.Single(x => x.Month == 3);

        Assert.Equal(900m, march.TotalTax);
        Assert.Equal(150m, march.TotalIrrfMonth);
        Assert.Equal(150m, march.TotalIrrfCompensated);
        Assert.Equal(750m, march.DarfDue);
    }

    [Fact]
    public async Task Should_Return_Empty_When_User_Has_No_Operations()
    {
        var repository = new FakeInvestmentOperationRepository([]);
        var ledgerRepository = new FakeLedgerEntryRepository([]);
        var handler = new GetIncomeTaxSummaryQueryHandler(repository, ledgerRepository);

        var result = await handler.HandleAsync(new GetIncomeTaxSummaryQuery(Guid.NewGuid()));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data!);
    }

    private static LedgerEntry CreateTaxEntry(string description, decimal amount, DateTime occurredAtUtc)
    {
        return new LedgerEntry(
            userId: Guid.NewGuid(),
            type: LedgerEntryType.Tax,
            description: description,
            grossAmount: amount,
            netAmount: amount,
            currency: "BRL",
            occurredAtUtc: occurredAtUtc,
            source: "test");
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

    private sealed class FakeLedgerEntryRepository(IReadOnlyCollection<LedgerEntry> entries)
        : ILedgerEntryRepository
    {
        public Task AddAsync(LedgerEntry entry, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<IReadOnlyCollection<LedgerEntry>> GetByUserIdAsync(
            Guid userId,
            DateTime? fromUtc = null,
            DateTime? toUtc = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult(entries);
    }
}
