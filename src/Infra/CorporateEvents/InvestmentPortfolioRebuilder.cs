namespace DinExApi.Infra;

internal sealed class InvestmentPortfolioRebuilder(
    IInvestmentOperationRepository investmentOperationRepository,
    ILedgerEntryRepository ledgerEntryRepository,
    ICorporateEventRepository corporateEventRepository,
    ICorporateEventProcessor corporateEventProcessor,
    IUnitOfWork unitOfWork) : IInvestmentPortfolioRebuilder
{
    public async Task<int> RebuildAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await investmentOperationRepository.DeleteByUserIdAsync(userId, cancellationToken);

        var entries = await ledgerEntryRepository.GetByUserIdAsync(userId, cancellationToken: cancellationToken);
        var movementEntries = entries
            .Where(x => x.Type is LedgerEntryType.Buy or LedgerEntryType.Sell)
            .Where(x => !string.IsNullOrWhiteSpace(x.AssetSymbol))
            .Where(x => x.Quantity.HasValue && x.Quantity.Value > 0)
            .Where(x => x.UnitPriceAmount.HasValue && x.UnitPriceAmount.Value >= 0)
            .OrderBy(x => x.OccurredAtUtc)
            .ThenBy(x => x.CreatedAt)
            .ToArray();

        foreach (var entry in movementEntries)
        {
            var operationType = entry.Type == LedgerEntryType.Buy ? OperationType.Buy : OperationType.Sell;
            var operation = new InvestmentOperation(
                userId: userId,
                assetSymbol: entry.AssetSymbol!,
                type: operationType,
                quantity: entry.Quantity!.Value,
                unitPrice: new Money(entry.UnitPriceAmount!.Value, entry.Currency),
                occurredAtUtc: entry.OccurredAtUtc);

            if (!operation.IsValid)
            {
                continue;
            }

            await investmentOperationRepository.AddAsync(operation, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var events = await corporateEventRepository.GetByUserIdAsync(userId, cancellationToken);
        var appliedOperations = 0;
        foreach (var corporateEvent in events.OrderBy(x => x.EffectiveAtUtc).ThenBy(x => x.CreatedAt))
        {
            appliedOperations += await corporateEventProcessor.ApplyAsync(corporateEvent, cancellationToken);
        }

        if (appliedOperations > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return appliedOperations;
    }
}
