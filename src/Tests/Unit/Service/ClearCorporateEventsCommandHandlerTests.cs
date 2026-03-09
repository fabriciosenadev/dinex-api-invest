namespace DinExApi.Tests;

public sealed class ClearCorporateEventsCommandHandlerTests
{
    [Fact]
    public async Task Should_Delete_Corporate_Events_And_Rebuild_Portfolio()
    {
        var corporateEventRepository = new FakeCorporateEventRepository();
        var rebuilder = new FakeInvestmentPortfolioRebuilder();
        var unitOfWork = new SpyUnitOfWork();
        var handler = new ClearCorporateEventsCommandHandler(corporateEventRepository, rebuilder, unitOfWork);
        var userId = Guid.NewGuid();

        var result = await handler.HandleAsync(new ClearCorporateEventsCommand(userId));

        Assert.True(result.Succeeded);
        Assert.Equal(userId, corporateEventRepository.LastDeletedUserId);
        Assert.Equal(userId, rebuilder.LastRebuildUserId);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    private sealed class FakeCorporateEventRepository : ICorporateEventRepository
    {
        public Guid LastDeletedUserId { get; private set; }

        public Task AddAsync(CorporateEvent entry, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task UpdateAsync(CorporateEvent entry, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<CorporateEvent?> GetByIdAsync(Guid userId, Guid eventId, CancellationToken cancellationToken = default)
            => Task.FromResult<CorporateEvent?>(null);

        public Task DeleteAsync(Guid userId, Guid eventId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            LastDeletedUserId = userId;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<CorporateEvent>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<CorporateEvent>>([]);
    }

    private sealed class FakeInvestmentPortfolioRebuilder : IInvestmentPortfolioRebuilder
    {
        public Guid LastRebuildUserId { get; private set; }

        public Task<int> RebuildAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            LastRebuildUserId = userId;
            return Task.FromResult(0);
        }
    }
}
