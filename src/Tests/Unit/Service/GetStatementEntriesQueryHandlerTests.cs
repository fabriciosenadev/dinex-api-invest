namespace DinExApi.Tests;

public sealed class GetStatementEntriesQueryHandlerTests
{
    [Fact]
    public async Task Should_Return_Mapped_Statement_Entries()
    {
        var userId = Guid.NewGuid();
        var repository = new FakeLedgerRepository([
            new LedgerEntry(
                userId: userId,
                type: LedgerEntryType.Income,
                description: "Dividendo",
                grossAmount: 25m,
                netAmount: 25m,
                currency: "BRL",
                occurredAtUtc: DateTime.UtcNow,
                source: "manual",
                assetSymbol: "PETR4")
        ]);

        var handler = new GetStatementEntriesQueryHandler(repository);
        var query = new GetStatementEntriesQuery(userId);

        var result = await handler.HandleAsync(query);

        Assert.True(result.Succeeded);
        Assert.Single(result.Data!);
        Assert.Equal("Dividendo", result.Data!.First().Description);
    }

    [Fact]
    public async Task Should_Return_Internal_Server_Error_When_Repository_Throws()
    {
        var handler = new GetStatementEntriesQueryHandler(new ThrowingLedgerRepository());

        var result = await handler.HandleAsync(new GetStatementEntriesQuery(Guid.NewGuid()));

        Assert.True(result.InternalServerError);
        Assert.Contains("Unexpected error", result.Errors.First());
    }

    [Fact]
    public void StatementEntryItem_And_Query_Should_Keep_Values()
    {
        var userId = Guid.NewGuid();
        var from = DateTime.UtcNow.Date;
        var to = from.AddDays(1);
        var query = new GetStatementEntriesQuery(userId, from, to);
        var item = new StatementEntryItem(
            Guid.NewGuid(),
            LedgerEntryType.Buy,
            "Compra PETR4",
            "PETR4",
            10,
            32.50m,
            325m,
            325m,
            "BRL",
            DateTime.UtcNow,
            "movement",
            null,
            null);

        Assert.Equal(userId, query.UserId);
        Assert.Equal(from, query.FromUtc);
        Assert.Equal(to, query.ToUtc);
        Assert.Equal("Compra PETR4", item.Description);
        Assert.Equal(LedgerEntryType.Buy, item.Type);
    }

    private sealed class FakeLedgerRepository(IReadOnlyCollection<LedgerEntry> entries) : ILedgerEntryRepository
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
        {
            var filtered = entries.Where(x => x.UserId == userId).ToArray();
            return Task.FromResult<IReadOnlyCollection<LedgerEntry>>(filtered);
        }
    }

    private sealed class ThrowingLedgerRepository : ILedgerEntryRepository
    {
        public Task AddAsync(LedgerEntry entry, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("boom");

        public Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("boom");

        public Task<IReadOnlyCollection<LedgerEntry>> GetByUserIdAsync(
            Guid userId,
            DateTime? fromUtc = null,
            DateTime? toUtc = null,
            CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("boom");
    }
}
