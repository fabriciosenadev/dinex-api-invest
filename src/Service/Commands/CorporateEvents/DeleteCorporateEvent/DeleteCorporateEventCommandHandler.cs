namespace DinExApi.Service;

public sealed class DeleteCorporateEventCommandHandler(
    ICorporateEventRepository corporateEventRepository,
    IInvestmentPortfolioRebuilder portfolioRebuilder,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteCorporateEventCommand, OperationResult<RegisterCorporateEventResult>>
{
    public async Task<OperationResult<RegisterCorporateEventResult>> HandleAsync(
        DeleteCorporateEventCommand command,
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

        await corporateEventRepository.DeleteAsync(command.UserId, command.EventId, cancellationToken);
        var affectedOperations = await portfolioRebuilder.RebuildAsync(command.UserId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        result.SetData(new RegisterCorporateEventResult(command.EventId, affectedOperations));
        return result;
    }
}
