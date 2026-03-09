
namespace DinExApi.Service;

public sealed class RegisterMovementCommandHandler(
    IInvestmentOperationRepository repository,
    ILedgerEntryRepository ledgerRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<RegisterMovementCommand, OperationResult<Guid>>
{
    public async Task<OperationResult<Guid>> HandleAsync(RegisterMovementCommand command, CancellationToken cancellationToken = default)
    {
        var result = new OperationResult<Guid>();

        try
        {
            var operation = new InvestmentOperation(
                command.UserId,
                command.AssetSymbol,
                command.Type,
                command.Quantity,
                new Money(command.UnitPrice, command.Currency),
                command.OccurredAtUtc);

            if (!operation.IsValid)
            {
                result.AddNotifications(operation.Notifications);
                return result;
            }

            await repository.AddAsync(operation, cancellationToken);

            var ledgerEntryType = command.Type == OperationType.Buy ? LedgerEntryType.Buy : LedgerEntryType.Sell;
            var grossAmount = command.Quantity * command.UnitPrice;
            var ledgerEntry = new LedgerEntry(
                userId: command.UserId,
                type: ledgerEntryType,
                description: null,
                grossAmount: grossAmount,
                netAmount: grossAmount,
                currency: command.Currency,
                occurredAtUtc: command.OccurredAtUtc,
                source: "movement",
                assetSymbol: command.AssetSymbol,
                quantity: command.Quantity,
                unitPriceAmount: command.UnitPrice,
                referenceId: operation.Id.ToString("N"),
                metadata: null);

            if (!ledgerEntry.IsValid)
            {
                result.AddNotifications(ledgerEntry.Notifications);
                return result;
            }

            await ledgerRepository.AddAsync(ledgerEntry, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            result.SetData(operation.Id);
            return result;
        }
        catch (DomainValidationException ex)
        {
            result.AddError(ex.Message);
            return result;
        }
        catch (Exception)
        {
            result.SetAsInternalServerError();
            result.AddError("Unexpected error while registering movement.");
            return result;
        }
    }
}
