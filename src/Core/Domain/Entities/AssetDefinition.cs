namespace DinExApi.Core;

public sealed class AssetDefinition : Entity
{
    private AssetDefinition(
        Guid userId,
        string symbol,
        AssetType type,
        string? notes,
        DateTime createdAt,
        DateTime? updatedAt,
        DateTime? deletedAt,
        Guid? id = null)
    {
        if (id.HasValue)
        {
            Id = id.Value;
        }

        UserId = userId;
        Symbol = (symbol ?? string.Empty).Trim().ToUpperInvariant();
        Type = type;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        DeletedAt = deletedAt;

        AddNotifications(
            new Contract<Notification>()
                .Requires()
                .IsNotEmpty(UserId, "AssetDefinition.UserId", "User is required.")
                .IsNotNullOrWhiteSpace(Symbol, "AssetDefinition.Symbol", "Asset symbol is required.")
                .IsTrue(Enum.IsDefined(Type), "AssetDefinition.Type", "Asset type is invalid."));

        if (Symbol.Length > 30)
        {
            AddNotification("AssetDefinition.Symbol", "Asset symbol must be up to 30 characters.");
        }

        if (!string.IsNullOrWhiteSpace(Notes) && Notes.Length > 500)
        {
            AddNotification("AssetDefinition.Notes", "Notes must be up to 500 characters.");
        }
    }

    public Guid UserId { get; private set; }
    public string Symbol { get; private set; }
    public AssetType Type { get; private set; }
    public string? Notes { get; private set; }

    public static AssetDefinition Create(Guid userId, string symbol, AssetType type, string? notes)
    {
        return new AssetDefinition(
            userId: userId,
            symbol: symbol,
            type: type,
            notes: notes,
            createdAt: DateTime.UtcNow,
            updatedAt: null,
            deletedAt: null);
    }

    public static AssetDefinition Restore(
        Guid id,
        Guid userId,
        string symbol,
        AssetType type,
        string? notes,
        DateTime createdAt,
        DateTime? updatedAt,
        DateTime? deletedAt)
    {
        return new AssetDefinition(
            userId: userId,
            symbol: symbol,
            type: type,
            notes: notes,
            createdAt: createdAt,
            updatedAt: updatedAt,
            deletedAt: deletedAt,
            id: id);
    }

    public void Update(string symbol, AssetType type, string? notes)
    {
        Symbol = (symbol ?? string.Empty).Trim().ToUpperInvariant();
        Type = type;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        UpdatedAt = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(Symbol))
        {
            AddNotification("AssetDefinition.Symbol", "Asset symbol is required.");
        }

        if (Symbol.Length > 30)
        {
            AddNotification("AssetDefinition.Symbol", "Asset symbol must be up to 30 characters.");
        }

        if (!Enum.IsDefined(Type))
        {
            AddNotification("AssetDefinition.Type", "Asset type is invalid.");
        }

        if (!string.IsNullOrWhiteSpace(Notes) && Notes.Length > 500)
        {
            AddNotification("AssetDefinition.Notes", "Notes must be up to 500 characters.");
        }
    }
}
