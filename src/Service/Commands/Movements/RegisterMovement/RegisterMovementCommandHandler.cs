
namespace DinExApi.Service;

public sealed class RegisterMovementCommandHandler(
    IInvestmentOperationRepository repository,
    IUnitOfWork unitOfWork) : ICommandHandler<RegisterMovementCommand, OperationResult<Guid>>
{
    public async Task<OperationResult<Guid>> HandleAsync(RegisterMovementCommand command, CancellationToken cancellationToken = default)
    {
        var result = new OperationResult<Guid>();

        try
        {
            var operation = new InvestmentOperation(
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
