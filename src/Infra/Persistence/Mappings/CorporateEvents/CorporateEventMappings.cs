namespace DinExApi.Infra;

internal static class CorporateEventMappings
{
    public static CorporateEventRecord ToRecord(this CorporateEvent entity)
    {
        return new CorporateEventRecord
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Type = (int)entity.Type,
            SourceAssetSymbol = entity.SourceAssetSymbol,
            TargetAssetSymbol = entity.TargetAssetSymbol,
            Factor = entity.Factor,
            CashPerSourceUnit = entity.CashPerSourceUnit,
            EffectiveAtUtc = entity.EffectiveAtUtc,
            Notes = entity.Notes,
            AppliedAtUtc = entity.AppliedAtUtc,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            DeletedAt = entity.DeletedAt
        };
    }

    public static CorporateEvent ToEntity(this CorporateEventRecord record)
    {
        return new CorporateEvent(
            userId: record.UserId,
            type: (CorporateEventType)record.Type,
            sourceAssetSymbol: record.SourceAssetSymbol,
            targetAssetSymbol: record.TargetAssetSymbol,
            factor: record.Factor,
            cashPerSourceUnit: record.CashPerSourceUnit,
            effectiveAtUtc: record.EffectiveAtUtc,
            notes: record.Notes,
            createdAt: record.CreatedAt,
            appliedAtUtc: record.AppliedAtUtc,
            id: record.Id);
    }
}
