namespace DinExApi.Tests;

public sealed class RegisterStatementEntryCommandHandlerTests
{
    [Fact]
    public async Task Should_Register_Statement_Entry()
    {
        var repository = new FakeLedgerRepository();
        var unitOfWork = new InMemoryUnitOfWork();
        var handler = new RegisterStatementEntryCommandHandler(repository, unitOfWork);

        var result = await handler.HandleAsync(new RegisterStatementEntryCommand(
            UserId: Guid.NewGuid(),
            Type: LedgerEntryType.Income,
            Description: "Dividendo mensal",
            GrossAmount: 120.45m,
            NetAmount: 120.45m,
            Currency: "BRL",
            OccurredAtUtc: DateTime.UtcNow,
            Source: "manual"));

        Assert.True(result.Succeeded);
        Assert.NotEqual(Guid.Empty, result.Data);
        Assert.Single(repository.Entries);
    }

    private sealed class FakeLedgerRepository : ILedgerEntryRepository
    {
        public List<LedgerEntry> Entries { get; } = [];

        public Task AddAsync(LedgerEntry entry, CancellationToken cancellationToken = default)
        {
            Entries.Add(entry);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<LedgerEntry>> GetByUserIdAsync(
            Guid userId,
            DateTime? fromUtc = null,
            DateTime? toUtc = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<LedgerEntry>>(Entries.Where(x => x.UserId == userId).ToArray());
        }
    }
}
