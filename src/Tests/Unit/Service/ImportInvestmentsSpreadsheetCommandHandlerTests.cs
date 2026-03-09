namespace DinExApi.Tests;

public sealed class ImportInvestmentsSpreadsheetCommandHandlerTests
{
    [Fact]
    public async Task Should_Import_Movements_And_Statement_Entries_In_Chronological_Order()
    {
        var parser = new FakeSpreadsheetParser(
        [
            new ImportedSpreadsheetRow(2, new DateTime(2021, 1, 10), "Venda", "Venda a mercado", "Entrada", "PETR4", 5, 31.50m, 157.50m, 157.50m, "BRL", "mov-2021.xlsx"),
            new ImportedSpreadsheetRow(3, new DateTime(2020, 1, 10), "Compra", "Compra a mercado", "Saida", "PETR4", 10, 30.00m, -300.00m, -300.00m, "BRL", "mov-2020.xlsx"),
            new ImportedSpreadsheetRow(4, new DateTime(2020, 6, 12), "Dividendo", "Rendimento", "Entrada", "PETR4", null, null, 20.00m, 20.00m, "BRL", "mov-2020.xlsx")
        ]);
        var movementRepository = new FakeInvestmentOperationRepository();
        var ledgerRepository = new FakeLedgerRepository();
        var unitOfWork = new SpyUnitOfWork();
        var handler = new ImportInvestmentsSpreadsheetCommandHandler(parser, movementRepository, ledgerRepository, unitOfWork);

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
        Assert.Equal(new DateTime(2020, 1, 10), movementRepository.Items.First().OccurredAtUtc.Date);
    }

    [Fact]
    public async Task Should_Skip_Invalid_Rows_And_Return_Warnings()
    {
        var parser = new FakeSpreadsheetParser(
        [
            new ImportedSpreadsheetRow(2, new DateTime(2020, 1, 10), "Compra", "Compra a mercado", "Saida", null, 10, 30.00m, -300.00m, -300.00m, "BRL", "mov.xlsx")
        ]);
        var movementRepository = new FakeInvestmentOperationRepository();
        var ledgerRepository = new FakeLedgerRepository();
        var unitOfWork = new SpyUnitOfWork();
        var handler = new ImportInvestmentsSpreadsheetCommandHandler(parser, movementRepository, ledgerRepository, unitOfWork);

        var result = await handler.HandleAsync(new ImportInvestmentsSpreadsheetCommand(
            Guid.NewGuid(),
            [new ImportInvestmentsSpreadsheetFile("mov.xlsx", [1])]));

        Assert.False(result.Succeeded);
        Assert.Contains("No valid rows", result.Errors.First());
        Assert.Empty(movementRepository.Items);
        Assert.Empty(ledgerRepository.Items);
    }

    [Fact]
    public async Task Should_Classify_CompraVenda_By_Direction_And_Allow_Bonus_Without_UnitPrice()
    {
        var parser = new FakeSpreadsheetParser(
        [
            new ImportedSpreadsheetRow(2, new DateTime(2025, 1, 10), "COMPRA / VENDA", "COMPRA / VENDA", "Credito", "CPTS11", 10, 8.50m, 85.00m, 85.00m, "BRL", "mov.xlsx"),
            new ImportedSpreadsheetRow(3, new DateTime(2025, 1, 11), "COMPRA / VENDA", "COMPRA / VENDA", "Debito", "CPTS11", 5, 8.80m, -44.00m, -44.00m, "BRL", "mov.xlsx"),
            new ImportedSpreadsheetRow(4, new DateTime(2025, 1, 12), "Bonificação em Ativos", "Bonificação em Ativos", "Credito", "ITUB4", 0.84m, null, null, null, "BRL", "mov.xlsx")
        ]);
        var movementRepository = new FakeInvestmentOperationRepository();
        var ledgerRepository = new FakeLedgerRepository();
        var unitOfWork = new SpyUnitOfWork();
        var handler = new ImportInvestmentsSpreadsheetCommandHandler(parser, movementRepository, ledgerRepository, unitOfWork);

        var result = await handler.HandleAsync(new ImportInvestmentsSpreadsheetCommand(
            Guid.NewGuid(),
            [new ImportInvestmentsSpreadsheetFile("mov.xlsx", [1, 2, 3])]));

        Assert.True(result.Succeeded);
        Assert.Equal(3, result.Data!.ImportedMovements);
        Assert.Equal(OperationType.Buy, movementRepository.Items[0].Type);
        Assert.Equal(OperationType.Sell, movementRepository.Items[1].Type);
        Assert.Equal(OperationType.Buy, movementRepository.Items[2].Type);
        Assert.Equal(0m, movementRepository.Items[2].UnitPrice.Amount);
    }

    [Fact]
    public async Task Should_Normalize_Temporary_Tickers_To_Base_Position_Ticker()
    {
        var parser = new FakeSpreadsheetParser(
        [
            new ImportedSpreadsheetRow(2, new DateTime(2025, 1, 10), "Bonificação em Ativos", "Bonificação em Ativos", "Credito", "AESB1", 4, null, null, null, "BRL", "mov.xlsx"),
            new ImportedSpreadsheetRow(3, new DateTime(2025, 1, 11), "Transferência - Liquidação", "Transferência - Liquidação", "Credito", "MXRF12", 72, 0.00m, 0.00m, 0.00m, "BRL", "mov.xlsx")
        ]);
        var movementRepository = new FakeInvestmentOperationRepository();
        var ledgerRepository = new FakeLedgerRepository();
        var unitOfWork = new SpyUnitOfWork();
        var handler = new ImportInvestmentsSpreadsheetCommandHandler(parser, movementRepository, ledgerRepository, unitOfWork);

        var result = await handler.HandleAsync(new ImportInvestmentsSpreadsheetCommand(
            Guid.NewGuid(),
            [new ImportInvestmentsSpreadsheetFile("mov.xlsx", [1, 2, 3])]));

        Assert.True(result.Succeeded);
        Assert.Equal("AESB3", movementRepository.Items[0].AssetSymbol);
        Assert.Equal("MXRF11", movementRepository.Items[1].AssetSymbol);
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
}
