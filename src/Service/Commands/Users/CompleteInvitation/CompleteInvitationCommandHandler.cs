namespace DinExApi.Service;

public sealed class CompleteInvitationCommandHandler(
    IUserRepository userRepository,
    IUserPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork) : ICommandHandler<CompleteInvitationCommand, OperationResult>
{
    private const int MaxActivationAttempts = 5;

    public async Task<OperationResult> HandleAsync(
        CompleteInvitationCommand command,
        CancellationToken cancellationToken = default)
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

            if (user.HasExceededActivationAttempts(MaxActivationAttempts))
            {
                result.AddError("Activation code attempts exceeded. Request a new code.");
                return result;
            }

            var normalizedCode = command.ActivationCode.Trim().ToUpperInvariant();
            var isValidCode = user.HasValidActivationCode()
                && string.Equals(user.ActivationCode, normalizedCode, StringComparison.Ordinal);

            if (!isValidCode)
            {
                user.RegisterActivationFailure(MaxActivationAttempts);
                await userRepository.UpdateAsync(user, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                result.AddError("Invalid or expired activation code.");
                return result;
            }

            if (string.IsNullOrWhiteSpace(command.Password) || command.Password.Length < 8)
            {
                result.AddError("Password must contain at least 8 characters.");
                return result;
            }

            if (!string.Equals(command.Password, command.ConfirmPassword, StringComparison.Ordinal))
            {
                result.AddError("Passwords must match.");
                return result;
            }

            user.SetPasswordHash(passwordHasher.HashPassword(command.Password));
            if (!user.IsValid)
            {
                result.AddNotifications(user.Notifications);
                return result;
            }

            user.Activate();
            await userRepository.UpdateAsync(user, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return result;
        }
        catch (Exception)
        {
            result.SetAsInternalServerError();
            result.AddError("Unexpected error while completing invitation.");
            return result;
        }
    }
}
