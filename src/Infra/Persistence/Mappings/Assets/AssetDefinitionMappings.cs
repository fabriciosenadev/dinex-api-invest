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
            notes: record.Notes,
            createdAt: record.CreatedAt,
            updatedAt: record.UpdatedAt,
            deletedAt: record.DeletedAt);
    }
}
