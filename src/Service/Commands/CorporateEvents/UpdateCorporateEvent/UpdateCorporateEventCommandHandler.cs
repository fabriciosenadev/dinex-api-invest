namespace DinExApi.Service;

public sealed class UpdateCorporateEventCommandHandler(
    ICorporateEventRepository corporateEventRepository,
    IInvestmentPortfolioRebuilder portfolioRebuilder,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateCorporateEventCommand, OperationResult<RegisterCorporateEventResult>>
{
    public async Task<OperationResult<RegisterCorporateEventResult>> HandleAsync(
        UpdateCorporateEventCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = new OperationResult<RegisterCorporateEventResult>();
        var existing = await corporateEventRepository.GetByIdAsync(command.UserId, command.EventId, cancellationToken);
        if (existing is null)
        {
            result.SetAsNotFound();
            result.AddError("Corporate event was not found.");
            return result;
        }

        var updated = new CorporateEvent(
            userId: command.UserId,
            type: command.Type,
            sourceAssetSymbol: command.SourceAssetSymbol,
            targetAssetSymbol: command.TargetAssetSymbol,
            factor: command.Factor,
            cashPerSourceUnit: command.CashPerSourceUnit,
            effectiveAtUtc: command.EffectiveAtUtc,
            notes: command.Notes,
            createdAt: existing.CreatedAt,
            appliedAtUtc: DateTime.UtcNow,
            id: existing.Id);

        if (!updated.IsValid)
        {
            result.AddErrors(updated.Notifications.Select(x => x.Message));
            return result;
        }

        await corporateEventRepository.UpdateAsync(updated, cancellationToken);
        var affectedOperations = await portfolioRebuilder.RebuildAsync(command.UserId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        result.SetData(new RegisterCorporateEventResult(updated.Id, affectedOperations));
        return result;
    }
}
