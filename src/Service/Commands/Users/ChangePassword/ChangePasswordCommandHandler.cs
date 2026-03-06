namespace DinExApi.Service;

public sealed class ChangePasswordCommandHandler(
    IUserRepository userRepository,
    IUserPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork) : ICommandHandler<ChangePasswordCommand, OperationResult>
{
    public async Task<OperationResult> HandleAsync(ChangePasswordCommand command, CancellationToken cancellationToken = default)
    {
        var result = new OperationResult();

        try
        {
            if (!ValidateNewPassword(command.NewPassword, command.ConfirmNewPassword, result))
            {
                return result;
            }

            var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
            if (user is null)
            {
                result.SetAsNotFound();
                result.AddError("User not found.");
                return result;
            }

            if (user.UserStatus != UserStatus.Active)
            {
                result.AddError("Account is not active.");
                return result;
            }

            var isCurrentPasswordValid = passwordHasher.VerifyPassword(user.Password, command.CurrentPassword);
            if (!isCurrentPasswordValid)
            {
                result.AddError("Current password is invalid.");
                return result;
            }

            user.SetPasswordHash(passwordHasher.HashPassword(command.NewPassword));
            user.ClearPasswordResetCode();
            user.RevokeRefreshToken();
            if (!user.IsValid)
            {
                result.AddNotifications(user.Notifications);
                return result;
            }

            await userRepository.UpdateAsync(user, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return result;
        }
        catch (Exception)
        {
            result.SetAsInternalServerError();
            result.AddError("Unexpected error while changing password.");
            return result;
        }
    }

    private static bool ValidateNewPassword(string newPassword, string confirmPassword, OperationResult result)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
        {
            result.AddError("New password must contain at least 8 characters.");
        }

        if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
        {
            result.AddError("New password and confirmation must match.");
        }

        return !result.HasErrors();
    }
}
