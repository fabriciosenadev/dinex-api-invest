namespace DinExApi.Tests;

public sealed class AssetDefinitionHandlersTests
{
    [Fact]
    public async Task Upsert_Should_Create_New_Asset_Definition_When_Symbol_Does_Not_Exist()
    {
        var repository = new FakeAssetDefinitionRepository();
        var unitOfWork = new SpyUnitOfWork();
        var handler = new UpsertAssetDefinitionCommandHandler(repository, unitOfWork);
        var userId = Guid.NewGuid();

        var result = await handler.HandleAsync(new UpsertAssetDefinitionCommand(userId, "GOLD11", AssetType.Etf, "ETF de ouro"));

        Assert.True(result.Succeeded);
        Assert.NotEqual(Guid.Empty, result.Data);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        var created = await repository.GetBySymbolAsync(userId, "GOLD11");
        Assert.NotNull(created);
        Assert.Equal(AssetType.Etf, created!.Type);
    }

    [Fact]
    public async Task Upsert_Should_Update_Existing_Asset_Definition_When_Symbol_Already_Exists()
    {
        var repository = new FakeAssetDefinitionRepository();
        var unitOfWork = new SpyUnitOfWork();
        var userId = Guid.NewGuid();
        var existing = AssetDefinition.Create(userId, "GOLD11", AssetType.Fii, null);
        await repository.AddAsync(existing);
        var handler = new UpsertAssetDefinitionCommandHandler(repository, unitOfWork);

        var result = await handler.HandleAsync(new UpsertAssetDefinitionCommand(userId, "GOLD11", AssetType.Etf, "Corrigido"));

        Assert.True(result.Succeeded);
        Assert.Equal(existing.Id, result.Data);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        var updated = await repository.GetBySymbolAsync(userId, "GOLD11");
        Assert.NotNull(updated);
        Assert.Equal(AssetType.Etf, updated!.Type);
        Assert.Equal("Corrigido", updated.Notes);
    }

    [Fact]
    public async Task Query_Should_Return_Ordered_Asset_Definitions()
    {
        var repository = new FakeAssetDefinitionRepository();
        var userId = Guid.NewGuid();
        await repository.AddAsync(AssetDefinition.Create(userId, "PETR4", AssetType.Stock, null));
        await repository.AddAsync(AssetDefinition.Create(userId, "ALZR11", AssetType.Fii, null));
        var handler = new GetAssetDefinitionsQueryHandler(repository);

        var result = await handler.HandleAsync(new GetAssetDefinitionsQuery(userId));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data!.Count);
        Assert.Equal("ALZR11", result.Data.First().Symbol);
        Assert.Equal("PETR4", result.Data.Last().Symbol);
    }

    private sealed class FakeAssetDefinitionRepository : IAssetDefinitionRepository
    {
        private readonly List<AssetDefinition> _items = [];

        public Task AddAsync(AssetDefinition assetDefinition, CancellationToken cancellationToken = default)
        {
            _items.Add(assetDefinition);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(AssetDefinition assetDefinition, CancellationToken cancellationToken = default)
        {
            var index = _items.FindIndex(x => x.UserId == assetDefinition.UserId && x.Id == assetDefinition.Id);
            if (index >= 0)
            {
                _items[index] = assetDefinition;
            }

            return Task.CompletedTask;
        }

        public Task<AssetDefinition?> GetByIdAsync(Guid userId, Guid assetDefinitionId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_items.FirstOrDefault(x => x.UserId == userId && x.Id == assetDefinitionId));
        }

        public Task<AssetDefinition?> GetBySymbolAsync(Guid userId, string symbol, CancellationToken cancellationToken = default)
        {
            var normalized = symbol.Trim().ToUpperInvariant();
            return Task.FromResult(_items.FirstOrDefault(x => x.UserId == userId && x.Symbol == normalized));
        }

        public Task<IReadOnlyCollection<AssetDefinition>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<AssetDefinition>>(_items.Where(x => x.UserId == userId).ToArray());
        }

        public Task DeleteAsync(Guid userId, Guid assetDefinitionId, CancellationToken cancellationToken = default)
        {
            _items.RemoveAll(x => x.UserId == userId && x.Id == assetDefinitionId);
            return Task.CompletedTask;
        }
    }
}
