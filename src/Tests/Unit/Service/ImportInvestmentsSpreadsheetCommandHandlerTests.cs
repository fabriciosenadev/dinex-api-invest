namespace DinExApi.Tests;

public sealed class ImportInvestmentsSpreadsheetCommandHandlerTests
{
    [Fact]
    public async Task Should_Import_Movements_And_Statement_Entries_In_Chronological_Order()
    {
        var parser = new FakeSpreadsheetParser(
        [
            new ImportedSpreadsheetRow(2, new DateTime(2021, 1, 10), "Transferência - Liquidação", "Transferência - Liquidação", "Debito", "PETR4", 5, 31.50m, 157.50m, 157.50m, "BRL", "mov-2021.xlsx"),
            new ImportedSpreadsheetRow(3, new DateTime(2020, 1, 10), "Transferência - Liquidação", "Transferência - Liquidação", "Credito", "PETR4", 10, 30.00m, -300.00m, -300.00m, "BRL", "mov-2020.xlsx"),
            new ImportedSpreadsheetRow(4, new DateTime(2020, 6, 12), "Dividendo", "Rendimento", "Entrada", "PETR4", null, null, 20.00m, 20.00m, "BRL", "mov-2020.xlsx")
        ]);
        var movementRepository = new FakeInvestmentOperationRepository();
        var ledgerRepository = new FakeLedgerRepository();
        var unitOfWork = new SpyUnitOfWork();
        var rebuilder = new SpyInvestmentPortfolioRebuilder();
        var handler = new ImportInvestmentsSpreadsheetCommandHandler(
            parser,
            new B3InvestmentMovementClassifier(),
            movementRepository,
            ledgerRepository,
            unitOfWork,
            rebuilder);

        var result = await handler.HandleAsync(new ImportInvestmentsSpreadsheetCommand(
            Guid.NewGuid(),
            [
                new ImportInvestmentsSpreadsheetFile("mov-2020.xlsx", [1, 2, 3]),
                new ImportInvestmentsSpreadsheetFile("mov-2021.xlsx", [4, 5, 6])
            ]));

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Data!.ProcessedFiles);
        Assert.Equal(3, result.Data.TotalRowsRead);
        Assert.Equal(2, result.Data.ImportedMovements);
        Assert.Equal(3, result.Data.ImportedStatementEntries);
        Assert.Equal(0, result.Data.SkippedRows);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Equal(1, rebuilder.CallCount);
        Assert.Equal(new DateTime(2020, 1, 10), movementRepository.Items.First().OccurredAtUtc.Date);
    }

    [Fact]
    public async Task Should_Skip_Invalid_Rows_And_Return_Warnings()
    {
        var parser = new FakeSpreadsheetParser(
        [
            new ImportedSpreadsheetRow(2, new DateTime(2020, 1, 10), "Transferência - Liquidação", "Transferência - Liquidação", "Credito", null, 10, 30.00m, -300.00m, -300.00m, "BRL", "mov.xlsx")
        ]);
        var movementRepository = new FakeInvestmentOperationRepository();
        var ledgerRepository = new FakeLedgerRepository();
        var unitOfWork = new SpyUnitOfWork();
        var handler = new ImportInvestmentsSpreadsheetCommandHandler(
            parser,
            new B3InvestmentMovementClassifier(),
            movementRepository,
            ledgerRepository,
            unitOfWork, new NoopInvestmentPortfolioRebuilder());

        var result = await handler.HandleAsync(new ImportInvestmentsSpreadsheetCommand(
            Guid.NewGuid(),
            [new ImportInvestmentsSpreadsheetFile("mov.xlsx", [1])]));

        Assert.False(result.Succeeded);
        Assert.Contains("No valid rows", result.Errors.First());
        Assert.Empty(movementRepository.Items);
        Assert.Empty(ledgerRepository.Items);
    }

    [Fact]
    public async Task Should_Create_Movement_Only_For_TransferenciaLiquidacao()
    {
        var parser = new FakeSpreadsheetParser(
        [
            new ImportedSpreadsheetRow(2, new DateTime(2025, 1, 10), "COMPRA / VENDA", "COMPRA / VENDA", "Credito", "CPTS11", 10, 8.50m, 85.00m, 85.00m, "BRL", "mov.xlsx"),
            new ImportedSpreadsheetRow(3, new DateTime(2025, 1, 11), "Transferência - Liquidação", "Transferência - Liquidação", "Debito", "CPTS11", 5, 8.80m, -44.00m, -44.00m, "BRL", "mov.xlsx"),
            new ImportedSpreadsheetRow(4, new DateTime(2025, 1, 12), "Bonificação em Ativos", "Bonificação em Ativos", "Credito", "ITUB4", 0.84m, null, null, null, "BRL", "mov.xlsx")
        ]);
        var movementRepository = new FakeInvestmentOperationRepository();
        var ledgerRepository = new FakeLedgerRepository();
        var unitOfWork = new SpyUnitOfWork();
        var handler = new ImportInvestmentsSpreadsheetCommandHandler(
            parser,
            new B3InvestmentMovementClassifier(),
            movementRepository,
            ledgerRepository,
            unitOfWork, new NoopInvestmentPortfolioRebuilder());

        var result = await handler.HandleAsync(new ImportInvestmentsSpreadsheetCommand(
            Guid.NewGuid(),
            [new ImportInvestmentsSpreadsheetFile("mov.xlsx", [1, 2, 3])]));

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.Data!.ImportedMovements);
        Assert.Single(movementRepository.Items);
        Assert.Equal(OperationType.Sell, movementRepository.Items[0].Type);
    }

    [Fact]
    public async Task Should_Keep_Imported_Asset_Symbol_Without_Forced_Normalization()
    {
        var parser = new FakeSpreadsheetParser(
        [
            new ImportedSpreadsheetRow(2, new DateTime(2025, 1, 10), "Bonificação em Ativos", "Bonificação em Ativos", "Credito", "AESB1", 4, null, null, null, "BRL", "mov.xlsx"),
            new ImportedSpreadsheetRow(3, new DateTime(2025, 1, 11), "Transferência - Liquidação", "Transferência - Liquidação", "Credito", "MXRF12", 72, 0.00m, 0.00m, 0.00m, "BRL", "mov.xlsx")
        ]);
        var movementRepository = new FakeInvestmentOperationRepository();
        var ledgerRepository = new FakeLedgerRepository();
        var unitOfWork = new SpyUnitOfWork();
        var handler = new ImportInvestmentsSpreadsheetCommandHandler(
            parser,
            new B3InvestmentMovementClassifier(),
            movementRepository,
            ledgerRepository,
            unitOfWork, new NoopInvestmentPortfolioRebuilder());

        var result = await handler.HandleAsync(new ImportInvestmentsSpreadsheetCommand(
            Guid.NewGuid(),
            [new ImportInvestmentsSpreadsheetFile("mov.xlsx", [1, 2, 3])]));

        Assert.True(result.Succeeded);
        Assert.Single(movementRepository.Items);
        Assert.Equal("MXRF12", movementRepository.Items[0].AssetSymbol);
    }

    [Fact]
    public async Task Should_Not_Create_Position_Movement_For_LeilaoDeFracao_Or_Cisao()
    {
        var parser = new FakeSpreadsheetParser(
        [
            new ImportedSpreadsheetRow(2, new DateTime(2025, 1, 10), "Leilão de Fração", "Leilão de Fração", "Credito", "ALUP4", 0.8m, null, 20m, 20m, "BRL", "mov.xlsx"),
            new ImportedSpreadsheetRow(3, new DateTime(2025, 1, 11), "Cisão", "Cisão", "Credito", "ITUB4", 3m, null, null, null, "BRL", "mov.xlsx"),
            new ImportedSpreadsheetRow(4, new DateTime(2025, 1, 12), "Transferência - Liquidação", "Transferência - Liquidação", "Credito", "ITUB4", 1m, 30m, 30m, 30m, "BRL", "mov.xlsx")
        ]);

        var movementRepository = new FakeInvestmentOperationRepository();
        var ledgerRepository = new FakeLedgerRepository();
        var unitOfWork = new SpyUnitOfWork();
        var handler = new ImportInvestmentsSpreadsheetCommandHandler(
            parser,
            new B3InvestmentMovementClassifier(),
            movementRepository,
            ledgerRepository,
            unitOfWork, new NoopInvestmentPortfolioRebuilder());

        var result = await handler.HandleAsync(new ImportInvestmentsSpreadsheetCommand(
            Guid.NewGuid(),
            [new ImportInvestmentsSpreadsheetFile("mov.xlsx", [1, 2, 3])]));

        Assert.True(result.Succeeded);
        Assert.Single(movementRepository.Items);
        Assert.Equal("ITUB4", movementRepository.Items[0].AssetSymbol);
        Assert.Equal(OperationType.Buy, movementRepository.Items[0].Type);
        Assert.Equal(3, ledgerRepository.Items.Count);
    }

    [Fact]
    public async Task Should_Not_Create_Movement_For_Standalone_Transferencia()
    {
        var parser = new FakeSpreadsheetParser(
        [
            new ImportedSpreadsheetRow(2, new DateTime(2022, 8, 19), "Transferência", "Transferência", "Credito", "GAME11", 1m, null, null, null, "BRL", "mov.xlsx")
        ]);

        var movementRepository = new FakeInvestmentOperationRepository();
        var ledgerRepository = new FakeLedgerRepository();
        var unitOfWork = new SpyUnitOfWork();
        var handler = new ImportInvestmentsSpreadsheetCommandHandler(
            parser,
            new B3InvestmentMovementClassifier(),
            movementRepository,
            ledgerRepository,
            unitOfWork, new NoopInvestmentPortfolioRebuilder());

        var result = await handler.HandleAsync(new ImportInvestmentsSpreadsheetCommand(
            Guid.NewGuid(),
            [new ImportInvestmentsSpreadsheetFile("mov.xlsx", [1, 2, 3])]));

        Assert.True(result.Succeeded);
        Assert.Empty(movementRepository.Items);
    }

    private sealed class FakeSpreadsheetParser(IReadOnlyCollection<ImportedSpreadsheetRow> rows) : IInvestmentSpreadsheetParser
    {
        public Task<IReadOnlyCollection<ImportedSpreadsheetRow>> ParseAsync(
            Stream stream,
            string fileName,
            CancellationToken cancellationToken = default)
        {
            var fileRows = rows.Where(x => string.Equals(x.FileName, fileName, StringComparison.OrdinalIgnoreCase)).ToArray();
            return Task.FromResult<IReadOnlyCollection<ImportedSpreadsheetRow>>(fileRows);
        }
    }

    private sealed class FakeInvestmentOperationRepository : IInvestmentOperationRepository
    {
        public List<InvestmentOperation> Items { get; } = [];

        public Task AddAsync(InvestmentOperation operation, CancellationToken cancellationToken = default)
        {
            Items.Add(operation);
            return Task.CompletedTask;
        }

        public Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<IReadOnlyCollection<PortfolioPosition>> GetPortfolioPositionsAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<PortfolioPosition>>([]);
    }

    private sealed class FakeLedgerRepository : ILedgerEntryRepository
    {
        public List<LedgerEntry> Items { get; } = [];

        public Task AddAsync(LedgerEntry entry, CancellationToken cancellationToken = default)
        {
            Items.Add(entry);
            return Task.CompletedTask;
        }

        public Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<IReadOnlyCollection<LedgerEntry>> GetByUserIdAsync(
            Guid userId,
            DateTime? fromUtc = null,
            DateTime? toUtc = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<LedgerEntry>>([]);
    }

    private sealed class NoopInvestmentPortfolioRebuilder : IInvestmentPortfolioRebuilder
    {
        public Task<int> RebuildAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult(0);
    }

    private sealed class SpyInvestmentPortfolioRebuilder : IInvestmentPortfolioRebuilder
    {
        public int CallCount { get; private set; }

        public Task<int> RebuildAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            CallCount += 1;
            return Task.FromResult(0);
        }
    }
}

