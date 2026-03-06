namespace DinExApi.Service;

public sealed class ResetPasswordCommandHandler(
    IUserRepository userRepository,
    IUserPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork) : ICommandHandler<ResetPasswordCommand, OperationResult>
{
    public async Task<OperationResult> HandleAsync(ResetPasswordCommand command, CancellationToken cancellationToken = default)
    {
        var result = new OperationResult();

        try
        {
            if (!ValidateNewPassword(command.NewPassword, command.ConfirmNewPassword, result))
            {
                return result;
            }

            var user = await userRepository.GetByEmailAsync(command.Email, cancellationToken);
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

            if (!user.HasValidPasswordResetCode(command.Code, DateTime.UtcNow))
            {
                result.AddError("Invalid or expired password reset code.");
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
            result.AddError("Unexpected error while resetting password.");
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
