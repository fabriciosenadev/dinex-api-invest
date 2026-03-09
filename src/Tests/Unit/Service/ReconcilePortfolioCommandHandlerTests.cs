namespace DinExApi.Tests;

public sealed class ReconcilePortfolioCommandHandlerTests
{
    [Fact]
    public async Task Should_Return_Ok_And_Divergence_Items()
    {
        var parser = new FakePortfolioParser(
        [
            new ImportedPortfolioPositionRow("ALUP4", 82m, "BRL"),
            new ImportedPortfolioPositionRow("ITUB4", 28.84m, "BRL")
        ]);
        var repository = new FakeInvestmentOperationRepository(
        [
            new PortfolioPosition("ALUP4", 82m, 10m, "BRL"),
            new PortfolioPosition("ITUB4", 32.34m, 27m, "BRL"),
            new PortfolioPosition("MXRF11", 10m, 9m, "BRL")
        ]);
        var handler = new ReconcilePortfolioCommandHandler(parser, repository, new PassthroughAssetAliasResolver());

        var result = await handler.HandleAsync(new ReconcilePortfolioCommand(
            Guid.NewGuid(),
            new ReconcilePortfolioSpreadsheetFile("posicao.xlsx", [1, 2, 3])));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(3, result.Data!.TotalAssets);
        Assert.Equal(1, result.Data.MatchedAssets);
        Assert.Equal(2, result.Data.DivergentAssets);
        Assert.Contains(result.Data.Assets, x => x.AssetSymbol == "ALUP4" && x.Status == "OK");
        Assert.Contains(result.Data.Assets, x => x.AssetSymbol == "ITUB4" && x.Status == "DIVERGENTE");
        Assert.Contains(result.Data.Assets, x => x.AssetSymbol == "MXRF11" && x.Reason.Contains("nao encontrado", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Should_Return_BadRequest_When_File_Is_Empty()
    {
        var parser = new FakePortfolioParser([]);
        var repository = new FakeInvestmentOperationRepository([]);
        var handler = new ReconcilePortfolioCommandHandler(parser, repository, new PassthroughAssetAliasResolver());

        var result = await handler.HandleAsync(new ReconcilePortfolioCommand(
            Guid.NewGuid(),
            new ReconcilePortfolioSpreadsheetFile("posicao.xlsx", [])));

        Assert.False(result.Succeeded);
        Assert.Contains("empty", result.Errors.First(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Should_Match_Canonical_Keys_For_FixedIncome_Assets()
    {
        var parser = new FakePortfolioParser(
        [
            new ImportedPortfolioPositionRow("25D02423916", 1000m, "BRL"),
            new ImportedPortfolioPositionRow("CDB223O735K", 1m, "BRL")
        ]);
        var repository = new FakeInvestmentOperationRepository(
        [
            new PortfolioPosition("LCI-25D02423916", 1000m, 1m, "BRL"),
            new PortfolioPosition("CDB-CDB223O735K", 1m, 1000m, "BRL")
        ]);
        var handler = new ReconcilePortfolioCommandHandler(parser, repository, new PassthroughAssetAliasResolver());

        var result = await handler.HandleAsync(new ReconcilePortfolioCommand(
            Guid.NewGuid(),
            new ReconcilePortfolioSpreadsheetFile("posicao.xlsx", [1, 2, 3])));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data!.TotalAssets);
        Assert.Equal(2, result.Data.MatchedAssets);
        Assert.Equal(0, result.Data.DivergentAssets);
    }

    [Fact]
    public async Task Should_Apply_Alias_Mapping_When_Comparing_Assets()
    {
        var parser = new FakePortfolioParser(
        [
            new ImportedPortfolioPositionRow("23A00041506", 10000m, "BRL")
        ]);
        var repository = new FakeInvestmentOperationRepository(
        [
            new PortfolioPosition("CDB-CDB223TZK4R", 10000m, 1m, "BRL")
        ]);
        var handler = new ReconcilePortfolioCommandHandler(
            parser,
            repository,
            new DictionaryAssetAliasResolver(new Dictionary<string, string>
            {
                ["23A00041506"] = "CDB-CDB223TZK4R"
            }));

        var result = await handler.HandleAsync(new ReconcilePortfolioCommand(
            Guid.NewGuid(),
            new ReconcilePortfolioSpreadsheetFile("posicao.xlsx", [1, 2, 3])));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data!.TotalAssets);
        Assert.Equal(1, result.Data.MatchedAssets);
        Assert.Equal(0, result.Data.DivergentAssets);
    }

    [Fact]
    public async Task Should_Ignore_Subscription_Rights_In_Reconciliation()
    {
        var parser = new FakePortfolioParser(
        [
            new ImportedPortfolioPositionRow("CPTS11", 100m, "BRL"),
            new ImportedPortfolioPositionRow("CPTS12", 50m, "BRL")
        ]);
        var repository = new FakeInvestmentOperationRepository(
        [
            new PortfolioPosition("CPTS11", 100m, 10m, "BRL"),
            new PortfolioPosition("CPTS12", 50m, 0m, "BRL")
        ]);
        var handler = new ReconcilePortfolioCommandHandler(parser, repository, new PassthroughAssetAliasResolver());

        var result = await handler.HandleAsync(new ReconcilePortfolioCommand(
            Guid.NewGuid(),
            new ReconcilePortfolioSpreadsheetFile("posicao.xlsx", [1, 2, 3])));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data!.TotalAssets);
        Assert.Single(result.Data.Assets);
        Assert.Equal("CPTS11", result.Data.Assets.First().AssetSymbol);
    }

    private sealed class FakePortfolioParser(IReadOnlyCollection<ImportedPortfolioPositionRow> rows)
        : IPortfolioPositionSpreadsheetParser
    {
        public Task<IReadOnlyCollection<ImportedPortfolioPositionRow>> ParseAsync(
            Stream stream,
            string fileName,
            CancellationToken cancellationToken = default)
            => Task.FromResult(rows);
    }

    private sealed class FakeInvestmentOperationRepository(IReadOnlyCollection<PortfolioPosition> positions)
        : IInvestmentOperationRepository
    {
        public Task AddAsync(InvestmentOperation operation, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<IReadOnlyCollection<PortfolioPosition>> GetPortfolioPositionsAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(positions);
    }

    private sealed class PassthroughAssetAliasResolver : IAssetAliasResolver
    {
        public string Resolve(string assetSymbol) => assetSymbol;
    }

    private sealed class DictionaryAssetAliasResolver(IReadOnlyDictionary<string, string> map) : IAssetAliasResolver
    {
        public string Resolve(string assetSymbol)
            => map.TryGetValue(assetSymbol, out var value) ? value : assetSymbol;
    }
}
