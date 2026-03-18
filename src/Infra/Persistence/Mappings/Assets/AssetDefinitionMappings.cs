namespace DinExApi.Infra;

internal static class AssetDefinitionMappings
{
    public static AssetDefinitionRecord ToRecord(this AssetDefinition entity)
    {
        return new AssetDefinitionRecord
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Symbol = entity.Symbol,
            Type = (int)entity.Type,
            Name = entity.Name,
            Document = entity.Document,
            Country = entity.Country,
            Currency = entity.Currency,
            Sector = entity.Sector,
            Segment = entity.Segment,
            ShareClass = entity.ShareClass,
            CvmCode = entity.CvmCode,
            FiiCategory = entity.FiiCategory,
            Administrator = entity.Administrator,
            Manager = entity.Manager,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            DeletedAt = entity.DeletedAt
        };
    }

    public static AssetDefinition ToEntity(this AssetDefinitionRecord record)
    {
        return AssetDefinition.Restore(
            id: record.Id,
            userId: record.UserId,
            symbol: record.Symbol,
            type: (AssetType)record.Type,
            name: record.Name,
            document: record.Document,
            country: record.Country,
            currency: record.Currency,
            sector: record.Sector,
            segment: record.Segment,
            shareClass: record.ShareClass,
            cvmCode: record.CvmCode,
            fiiCategory: record.FiiCategory,
            administrator: record.Administrator,
            manager: record.Manager,
            notes: record.Notes,
            createdAt: record.CreatedAt,
            updatedAt: record.UpdatedAt,
            deletedAt: record.DeletedAt);
    }
}
