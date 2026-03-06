namespace DinExApi.Service;

public sealed class ResendActivationCodeCommandHandler(
    IUserRepository userRepository,
    IUserActivationEmailSender activationEmailSender,
    IUnitOfWork unitOfWork) : ICommandHandler<ResendActivationCodeCommand, OperationResult>
{
    public async Task<OperationResult> HandleAsync(ResendActivationCodeCommand command, CancellationToken cancellationToken = default)
    {
        var result = new OperationResult();

        try
        {
            var user = await userRepository.GetByEmailAsync(command.Email, cancellationToken);
            if (user is null)
            {
                result.SetAsNotFound();
                result.AddError("User not found.");
                return result;
            }

            if (user.UserStatus == UserStatus.Active)
            {
                result.AddError("Account is already active.");
                return result;
            }

            user.AssignActivationCode(Guid.NewGuid().ToString("N")[..8].ToUpperInvariant());
            if (!user.IsValid)
            {
                result.AddNotifications(user.Notifications);
                return result;
            }

            await userRepository.UpdateAsync(user, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await activationEmailSender.SendActivationCodeAsync(
                user.FullName,
                user.Email,
                user.ActivationCode ?? string.Empty,
                cancellationToken);

            return result;
        }
        catch (Exception)
        {
            result.SetAsInternalServerError();
            result.AddError("Unexpected error while resending activation code.");
            return result;
        }
    }
}
