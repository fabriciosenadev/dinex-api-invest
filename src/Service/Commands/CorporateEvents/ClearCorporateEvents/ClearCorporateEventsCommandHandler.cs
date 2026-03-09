namespace DinExApi.Service;

public sealed class ClearCorporateEventsCommandHandler(
    ICorporateEventRepository corporateEventRepository,
    IInvestmentPortfolioRebuilder portfolioRebuilder,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ClearCorporateEventsCommand, OperationResult>
{
    public async Task<OperationResult> HandleAsync(
        ClearCorporateEventsCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = new OperationResult();

        try
        {
            await corporateEventRepository.DeleteByUserIdAsync(command.UserId, cancellationToken);
            await portfolioRebuilder.RebuildAsync(command.UserId, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return result;
        }
        catch (Exception)
        {
            result.SetAsInternalServerError();
            result.AddError("Unexpected error while clearing corporate events.");
            return result;
        }
    }
}
