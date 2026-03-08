namespace DinExApi.Infra;

internal static class LedgerEntryMappings
{
    public static LedgerEntryRecord ToRecord(this LedgerEntry entity)
    {
        return new LedgerEntryRecord
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Type = (int)entity.Type,
            Description = entity.Description,
            AssetSymbol = entity.AssetSymbol,
            Quantity = entity.Quantity,
            UnitPriceAmount = entity.UnitPriceAmount,
            GrossAmount = entity.GrossAmount,
            NetAmount = entity.NetAmount,
            Currency = entity.Currency,
            OccurredAtUtc = entity.OccurredAtUtc,
            Source = entity.Source,
            ReferenceId = entity.ReferenceId,
            Metadata = entity.Metadata,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            DeletedAt = entity.DeletedAt
        };
    }

    public static LedgerEntry ToEntity(this LedgerEntryRecord record)
    {
        return new LedgerEntry(
            userId: record.UserId,
            type: (LedgerEntryType)record.Type,
            description: record.Description,
            grossAmount: record.GrossAmount,
            netAmount: record.NetAmount,
            currency: record.Currency,
            occurredAtUtc: record.OccurredAtUtc,
            source: record.Source,
            assetSymbol: record.AssetSymbol,
            quantity: record.Quantity,
            unitPriceAmount: record.UnitPriceAmount,
            referenceId: record.ReferenceId,
            metadata: record.Metadata,
            createdAt: record.CreatedAt,
            id: record.Id);
    }
}
