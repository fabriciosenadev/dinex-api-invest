namespace DinExApi.Infra;

internal sealed class SqliteCorporateEventProcessor(IRepository<InvestmentOperationRecord> repository) : ICorporateEventProcessor
{
    public async Task<int> ApplyAsync(CorporateEvent corporateEvent, CancellationToken cancellationToken = default)
    {
        var sourceAsset = corporateEvent.SourceAssetSymbol.Trim().ToUpperInvariant();
        var targetAsset = corporateEvent.TargetAssetSymbol?.Trim().ToUpperInvariant();

        var operations = await repository.Query(asNoTracking: false)
            .Where(x =>
                x.UserId == corporateEvent.UserId &&
                x.AssetSymbol == sourceAsset &&
                x.OccurredAtUtc <= corporateEvent.EffectiveAtUtc)
            .ToListAsync(cancellationToken);

        if (operations.Count == 0)
        {
            return 0;
        }

        var adjustedOperations = new List<InvestmentOperationRecord>(operations.Count);
        foreach (var operation in operations)
        {
            var assetSymbol = operation.AssetSymbol;
            var quantity = operation.Quantity;
            var unitPriceAmount = operation.UnitPriceAmount;

            switch (corporateEvent.Type)
            {
                case CorporateEventType.TickerChange:
                    if (!string.IsNullOrWhiteSpace(targetAsset))
                    {
                        assetSymbol = targetAsset;
                    }
                    quantity = Math.Round(operation.Quantity * corporateEvent.Factor, 6, MidpointRounding.AwayFromZero);
                    unitPriceAmount = Math.Round(
                        corporateEvent.Factor == 0 ? operation.UnitPriceAmount : operation.UnitPriceAmount / corporateEvent.Factor,
                        6,
                        MidpointRounding.AwayFromZero);
                    break;
                case CorporateEventType.Split:
                    quantity = Math.Round(operation.Quantity * corporateEvent.Factor, 6, MidpointRounding.AwayFromZero);
                    unitPriceAmount = Math.Round(operation.UnitPriceAmount / corporateEvent.Factor, 6, MidpointRounding.AwayFromZero);
                    break;
                case CorporateEventType.ReverseSplit:
                    quantity = Math.Round(operation.Quantity / corporateEvent.Factor, 6, MidpointRounding.AwayFromZero);
                    unitPriceAmount = Math.Round(operation.UnitPriceAmount * corporateEvent.Factor, 6, MidpointRounding.AwayFromZero);
                    break;
                case CorporateEventType.IncorporationWithCash:
                    if (!string.IsNullOrWhiteSpace(targetAsset))
                    {
                        assetSymbol = targetAsset;
                    }
                    quantity = Math.Round(operation.Quantity * corporateEvent.Factor, 6, MidpointRounding.AwayFromZero);
                    unitPriceAmount = Math.Round(operation.UnitPriceAmount / corporateEvent.Factor, 6, MidpointRounding.AwayFromZero);
                    break;
            }

            adjustedOperations.Add(new InvestmentOperationRecord
            {
                Id = Guid.NewGuid(),
                UserId = operation.UserId,
                AssetSymbol = assetSymbol,
                Type = operation.Type,
                Quantity = quantity,
                UnitPriceAmount = unitPriceAmount,
                Currency = operation.Currency,
                OccurredAtUtc = operation.OccurredAtUtc
            });
        }

        repository.DeleteRange(operations);
        await repository.AddRangeAsync(adjustedOperations, cancellationToken);
        return operations.Count;
    }
}
