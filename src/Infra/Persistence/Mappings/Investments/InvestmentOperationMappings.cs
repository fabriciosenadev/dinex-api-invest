namespace DinExApi.Infra;

internal static class InvestmentOperationMappings
{
    public static InvestmentOperationRecord ToRecord(this InvestmentOperation entity)
    {
        return new InvestmentOperationRecord
        {
            Id = entity.Id,
            UserId = entity.UserId,
            AssetSymbol = entity.AssetSymbol,
            Type = (int)entity.Type,
            Quantity = entity.Quantity,
            UnitPriceAmount = entity.UnitPrice.Amount,
            Currency = entity.UnitPrice.Currency,
            OccurredAtUtc = entity.OccurredAtUtc
        };
    }
}
