namespace DinExApi.Tests;

public sealed class ClearAllEntriesCommandHandlerTests
{
    [Fact]
    public async Task Should_Delete_Investment_And_Statement_Entries()
    {
        var investmentRepository = new FakeInvestmentRepository();
        var ledgerRepository = new FakeLedgerRepository();
        var corporateEventRepository = new FakeCorporateEventRepository();
        var unitOfWork = new SpyUnitOfWork();
        var handler = new ClearAllEntriesCommandHandler(investmentRepository, ledgerRepository, corporateEventRepository, unitOfWork);
        var userId = Guid.NewGuid();

        var result = await handler.HandleAsync(new ClearAllEntriesCommand(userId));

        Assert.True(result.Succeeded);
        Assert.Equal(userId, investmentRepository.LastUserId);
        Assert.Equal(userId, ledgerRepository.LastUserId);
        Assert.Equal(userId, corporateEventRepository.LastUserId);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    private sealed class FakeInvestmentRepository : IInvestmentOperationRepository
    {
        public Guid LastUserId { get; private set; }

        public Task AddAsync(InvestmentOperation operation, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            LastUserId = userId;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<PortfolioPosition>> GetPortfolioPositionsAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<PortfolioPosition>>([]);
    }

    private sealed class FakeLedgerRepository : ILedgerEntryRepository
    {
        public Guid LastUserId { get; private set; }

        public Task AddAsync(LedgerEntry entry, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            LastUserId = userId;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<LedgerEntry>> GetByUserIdAsync(
            Guid userId,
            DateTime? fromUtc = null,
            DateTime? toUtc = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<LedgerEntry>>([]);
    }

    private sealed class FakeCorporateEventRepository : ICorporateEventRepository
    {
        public Guid LastUserId { get; private set; }

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
            LastUserId = userId;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<CorporateEvent>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<CorporateEvent>>([]);
    }
}
