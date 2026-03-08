namespace DinExApi.Service;

public sealed class RegisterStatementEntryCommandHandler(
    ILedgerEntryRepository repository,
    IUnitOfWork unitOfWork) : ICommandHandler<RegisterStatementEntryCommand, OperationResult<Guid>>
{
    public async Task<OperationResult<Guid>> HandleAsync(
        RegisterStatementEntryCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = new OperationResult<Guid>();

        try
        {
            var entry = new LedgerEntry(
                userId: command.UserId,
                type: command.Type,
                description: command.Description,
                grossAmount: command.GrossAmount,
                netAmount: command.NetAmount,
                currency: command.Currency,
                occurredAtUtc: command.OccurredAtUtc,
                source: command.Source,
                assetSymbol: command.AssetSymbol,
                quantity: command.Quantity,
                unitPriceAmount: command.UnitPriceAmount,
                referenceId: command.ReferenceId,
                metadata: command.Metadata);

            if (!entry.IsValid)
            {
                result.AddNotifications(entry.Notifications);
                return result;
            }

            await repository.AddAsync(entry, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            result.SetData(entry.Id);
            return result;
        }
        catch (Exception)
        {
            result.SetAsInternalServerError();
            result.AddError("Unexpected error while registering statement entry.");
            return result;
        }
    }
}
