namespace DinExApi.Core;

public sealed class AssetDefinition : Entity
{
    private AssetDefinition(
        Guid userId,
        string symbol,
        AssetType type,
        string? name,
        string? document,
        string? country,
        string? currency,
        string? sector,
        string? segment,
        string? shareClass,
        string? cvmCode,
        string? fiiCategory,
        string? administrator,
        string? manager,
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
        Name = NormalizeOptional(name);
        Document = NormalizeOptional(document)?.ToUpperInvariant();
        Country = NormalizeOptional(country);
        Currency = NormalizeOptional(currency)?.ToUpperInvariant();
        Sector = NormalizeOptional(sector);
        Segment = NormalizeOptional(segment);
        ShareClass = NormalizeOptional(shareClass);
        CvmCode = NormalizeOptional(cvmCode);
        FiiCategory = NormalizeOptional(fiiCategory);
        Administrator = NormalizeOptional(administrator);
        Manager = NormalizeOptional(manager);
        Notes = NormalizeOptional(notes);
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

        ValidateLength(Name, 150, "AssetDefinition.Name", "Asset name must be up to 150 characters.");
        ValidateLength(Document, 30, "AssetDefinition.Document", "Document must be up to 30 characters.");
        ValidateLength(Country, 40, "AssetDefinition.Country", "Country must be up to 40 characters.");
        ValidateLength(Currency, 10, "AssetDefinition.Currency", "Currency must be up to 10 characters.");
        ValidateLength(Sector, 80, "AssetDefinition.Sector", "Sector must be up to 80 characters.");
        ValidateLength(Segment, 80, "AssetDefinition.Segment", "Segment must be up to 80 characters.");
        ValidateLength(ShareClass, 40, "AssetDefinition.ShareClass", "Share class must be up to 40 characters.");
        ValidateLength(CvmCode, 30, "AssetDefinition.CvmCode", "CVM code must be up to 30 characters.");
        ValidateLength(FiiCategory, 80, "AssetDefinition.FiiCategory", "FII category must be up to 80 characters.");
        ValidateLength(Administrator, 150, "AssetDefinition.Administrator", "Administrator must be up to 150 characters.");
        ValidateLength(Manager, 150, "AssetDefinition.Manager", "Manager must be up to 150 characters.");
    }

    public Guid UserId { get; private set; }
    public string Symbol { get; private set; }
    public AssetType Type { get; private set; }
    public string? Name { get; private set; }
    public string? Document { get; private set; }
    public string? Country { get; private set; }
    public string? Currency { get; private set; }
    public string? Sector { get; private set; }
    public string? Segment { get; private set; }
    public string? ShareClass { get; private set; }
    public string? CvmCode { get; private set; }
    public string? FiiCategory { get; private set; }
    public string? Administrator { get; private set; }
    public string? Manager { get; private set; }
    public string? Notes { get; private set; }

    public static AssetDefinition Create(
        Guid userId,
        string symbol,
        AssetType type,
        string? name,
        string? document,
        string? country,
        string? currency,
        string? sector,
        string? segment,
        string? shareClass,
        string? cvmCode,
        string? fiiCategory,
        string? administrator,
        string? manager,
        string? notes)
    {
        return new AssetDefinition(
            userId: userId,
            symbol: symbol,
            type: type,
            name: name,
            document: document,
            country: country,
            currency: currency,
            sector: sector,
            segment: segment,
            shareClass: shareClass,
            cvmCode: cvmCode,
            fiiCategory: fiiCategory,
            administrator: administrator,
            manager: manager,
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
        string? name,
        string? document,
        string? country,
        string? currency,
        string? sector,
        string? segment,
        string? shareClass,
        string? cvmCode,
        string? fiiCategory,
        string? administrator,
        string? manager,
        string? notes,
        DateTime createdAt,
        DateTime? updatedAt,
        DateTime? deletedAt)
    {
        return new AssetDefinition(
            userId: userId,
            symbol: symbol,
            type: type,
            name: name,
            document: document,
            country: country,
            currency: currency,
            sector: sector,
            segment: segment,
            shareClass: shareClass,
            cvmCode: cvmCode,
            fiiCategory: fiiCategory,
            administrator: administrator,
            manager: manager,
            notes: notes,
            createdAt: createdAt,
            updatedAt: updatedAt,
            deletedAt: deletedAt,
            id: id);
    }

    public void Update(
        string symbol,
        AssetType type,
        string? name,
        string? document,
        string? country,
        string? currency,
        string? sector,
        string? segment,
        string? shareClass,
        string? cvmCode,
        string? fiiCategory,
        string? administrator,
        string? manager,
        string? notes)
    {
        Symbol = (symbol ?? string.Empty).Trim().ToUpperInvariant();
        Type = type;
        Name = NormalizeOptional(name);
        Document = NormalizeOptional(document)?.ToUpperInvariant();
        Country = NormalizeOptional(country);
        Currency = NormalizeOptional(currency)?.ToUpperInvariant();
        Sector = NormalizeOptional(sector);
        Segment = NormalizeOptional(segment);
        ShareClass = NormalizeOptional(shareClass);
        CvmCode = NormalizeOptional(cvmCode);
        FiiCategory = NormalizeOptional(fiiCategory);
        Administrator = NormalizeOptional(administrator);
        Manager = NormalizeOptional(manager);
        Notes = NormalizeOptional(notes);
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

        ValidateLength(Name, 150, "AssetDefinition.Name", "Asset name must be up to 150 characters.");
        ValidateLength(Document, 30, "AssetDefinition.Document", "Document must be up to 30 characters.");
        ValidateLength(Country, 40, "AssetDefinition.Country", "Country must be up to 40 characters.");
        ValidateLength(Currency, 10, "AssetDefinition.Currency", "Currency must be up to 10 characters.");
        ValidateLength(Sector, 80, "AssetDefinition.Sector", "Sector must be up to 80 characters.");
        ValidateLength(Segment, 80, "AssetDefinition.Segment", "Segment must be up to 80 characters.");
        ValidateLength(ShareClass, 40, "AssetDefinition.ShareClass", "Share class must be up to 40 characters.");
        ValidateLength(CvmCode, 30, "AssetDefinition.CvmCode", "CVM code must be up to 30 characters.");
        ValidateLength(FiiCategory, 80, "AssetDefinition.FiiCategory", "FII category must be up to 80 characters.");
        ValidateLength(Administrator, 150, "AssetDefinition.Administrator", "Administrator must be up to 150 characters.");
        ValidateLength(Manager, 150, "AssetDefinition.Manager", "Manager must be up to 150 characters.");
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private void ValidateLength(string? value, int maxLength, string key, string message)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Length > maxLength)
        {
            AddNotification(key, message);
        }
    }
}
