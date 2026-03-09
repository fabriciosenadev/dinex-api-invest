
namespace DinExApi.Infra;

public sealed class InMemoryDataStore
{
    private readonly List<InvestmentOperation> _operations = [];
    private readonly List<LedgerEntry> _ledgerEntries = [];
    private readonly List<User> _users = [];
    private readonly object _lock = new();

    public void Add(InvestmentOperation operation)
    {
        lock (_lock)
        {
            _operations.Add(operation);
        }
    }

    public IReadOnlyCollection<InvestmentOperation> Snapshot(Guid userId)
    {
        lock (_lock)
        {
            return _operations.Where(x => x.UserId == userId).ToArray();
        }
    }

    public void DeleteOperationsByUserId(Guid userId)
    {
        lock (_lock)
        {
            _operations.RemoveAll(x => x.UserId == userId);
        }
    }

    public void AddLedgerEntry(LedgerEntry entry)
    {
        lock (_lock)
        {
            _ledgerEntries.Add(entry);
        }
    }

    public IReadOnlyCollection<LedgerEntry> SnapshotLedgerEntries(Guid userId)
    {
        lock (_lock)
        {
            return _ledgerEntries.Where(x => x.UserId == userId).ToArray();
        }
    }

    public void DeleteLedgerEntriesByUserId(Guid userId)
    {
        lock (_lock)
        {
            _ledgerEntries.RemoveAll(x => x.UserId == userId);
        }
    }

    public void AddUser(User user)
    {
        lock (_lock)
        {
            _users.Add(user);
        }
    }

    public User? FindUserByEmail(string email)
    {
        lock (_lock)
        {
            return _users.FirstOrDefault(x => string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase));
        }
    }

    public User? FindUserById(Guid id)
    {
        lock (_lock)
        {
            return _users.FirstOrDefault(x => x.Id == id);
        }
    }

    public User? FindUserByRefreshTokenHash(string refreshTokenHash)
    {
        lock (_lock)
        {
            return _users.FirstOrDefault(x => string.Equals(x.RefreshTokenHash, refreshTokenHash, StringComparison.Ordinal));
        }
    }
}
