namespace DinExApi.Service;

public sealed class ClearAllEntriesCommandHandler(
    IInvestmentOperationRepository investmentOperationRepository,
    ILedgerEntryRepository ledgerEntryRepository,
    ICorporateEventRepository corporateEventRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<ClearAllEntriesCommand, OperationResult>
{
    public async Task<OperationResult> HandleAsync(ClearAllEntriesCommand command, CancellationToken cancellationToken = default)
    {
        var result = new OperationResult();

        try
        {
            await investmentOperationRepository.DeleteByUserIdAsync(command.UserId, cancellationToken);
            await ledgerEntryRepository.DeleteByUserIdAsync(command.UserId, cancellationToken);
            await corporateEventRepository.DeleteByUserIdAsync(command.UserId, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return result;
        }
        catch (Exception)
        {
            result.SetAsInternalServerError();
            result.AddError("Unexpected error while clearing user entries.");
            return result;
        }
    }
}
