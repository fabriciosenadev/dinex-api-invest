namespace DinExApi.Service;

public sealed class RegisterCorporateEventCommandHandler(
    ICorporateEventRepository corporateEventRepository,
    ICorporateEventProcessor corporateEventProcessor,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RegisterCorporateEventCommand, OperationResult<RegisterCorporateEventResult>>
{
    public async Task<OperationResult<RegisterCorporateEventResult>> HandleAsync(
        RegisterCorporateEventCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = new OperationResult<RegisterCorporateEventResult>();

        var entity = new CorporateEvent(
            userId: command.UserId,
            type: command.Type,
            sourceAssetSymbol: command.SourceAssetSymbol,
            targetAssetSymbol: command.TargetAssetSymbol,
            factor: command.Factor,
            effectiveAtUtc: command.EffectiveAtUtc,
            notes: command.Notes);

        if (!entity.IsValid)
        {
            result.AddErrors(entity.Notifications.Select(x => x.Message));
            return result;
        }

        await corporateEventRepository.AddAsync(entity, cancellationToken);
        var affectedOperations = await corporateEventProcessor.ApplyAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        result.SetData(new RegisterCorporateEventResult(entity.Id, affectedOperations));
        return result;
    }
}
