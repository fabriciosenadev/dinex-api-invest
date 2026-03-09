
namespace DinExApi.Infra;

public sealed class InMemoryDataStore
{
    private readonly List<InvestmentOperation> _operations = [];
    private readonly List<LedgerEntry> _ledgerEntries = [];
    private readonly List<CorporateEvent> _corporateEvents = [];
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

    public void AddCorporateEvent(CorporateEvent corporateEvent)
    {
        lock (_lock)
        {
            _corporateEvents.Add(corporateEvent);
        }
    }

    public IReadOnlyCollection<CorporateEvent> SnapshotCorporateEvents(Guid userId)
    {
        lock (_lock)
        {
            return _corporateEvents
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.EffectiveAtUtc)
                .ToArray();
        }
    }

    public void DeleteCorporateEventsByUserId(Guid userId)
    {
        lock (_lock)
        {
            _corporateEvents.RemoveAll(x => x.UserId == userId);
        }
    }

    public CorporateEvent? FindCorporateEventById(Guid userId, Guid eventId)
    {
        lock (_lock)
        {
            return _corporateEvents.FirstOrDefault(x => x.UserId == userId && x.Id == eventId);
        }
    }

    public void UpdateCorporateEvent(CorporateEvent corporateEvent)
    {
        lock (_lock)
        {
            var index = _corporateEvents.FindIndex(x => x.UserId == corporateEvent.UserId && x.Id == corporateEvent.Id);
            if (index >= 0)
            {
                _corporateEvents[index] = corporateEvent;
            }
        }
    }

    public void DeleteCorporateEventById(Guid userId, Guid eventId)
    {
        lock (_lock)
        {
            _corporateEvents.RemoveAll(x => x.UserId == userId && x.Id == eventId);
        }
    }

    public int ApplyCorporateEvent(CorporateEvent corporateEvent)
    {
        lock (_lock)
        {
            var affectedIndexes = _operations
                .Select((operation, index) => new { operation, index })
                .Where(x =>
                    x.operation.UserId == corporateEvent.UserId &&
                    x.operation.AssetSymbol == corporateEvent.SourceAssetSymbol &&
                    x.operation.OccurredAtUtc <= corporateEvent.EffectiveAtUtc)
                .ToArray();

            foreach (var target in affectedIndexes)
            {
                _operations[target.index] = BuildAdjustedOperation(target.operation, corporateEvent);
            }

            return affectedIndexes.Length;
        }
    }

    private static InvestmentOperation BuildAdjustedOperation(InvestmentOperation operation, CorporateEvent corporateEvent)
    {
        var assetSymbol = corporateEvent.Type == CorporateEventType.TickerChange
            ? (corporateEvent.TargetAssetSymbol ?? operation.AssetSymbol)
            : operation.AssetSymbol;

        var quantity = operation.Quantity;
        var unitPrice = operation.UnitPrice.Amount;

        if (corporateEvent.Type == CorporateEventType.TickerChange)
        {
            quantity = Math.Round(quantity * corporateEvent.Factor, 6, MidpointRounding.AwayFromZero);
            unitPrice = Math.Round(
                corporateEvent.Factor == 0 ? unitPrice : unitPrice / corporateEvent.Factor,
                6,
                MidpointRounding.AwayFromZero);
        }

        if (corporateEvent.Type == CorporateEventType.Split)
        {
            quantity = Math.Round(quantity * corporateEvent.Factor, 6, MidpointRounding.AwayFromZero);
            unitPrice = Math.Round(unitPrice / corporateEvent.Factor, 6, MidpointRounding.AwayFromZero);
        }
        else if (corporateEvent.Type == CorporateEventType.ReverseSplit)
        {
            quantity = Math.Round(quantity / corporateEvent.Factor, 6, MidpointRounding.AwayFromZero);
            unitPrice = Math.Round(unitPrice * corporateEvent.Factor, 6, MidpointRounding.AwayFromZero);
        }

        return new InvestmentOperation(
            operation.UserId,
            assetSymbol,
            operation.Type,
            quantity,
            new Money(unitPrice, operation.UnitPrice.Currency),
            operation.OccurredAtUtc);
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
