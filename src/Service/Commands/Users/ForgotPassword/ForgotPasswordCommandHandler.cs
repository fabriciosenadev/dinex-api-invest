namespace DinExApi.Service;

public sealed class ForgotPasswordCommandHandler(
    IUserRepository userRepository,
    IUserPasswordResetEmailSender passwordResetEmailSender,
    IUnitOfWork unitOfWork) : ICommandHandler<ForgotPasswordCommand, OperationResult>
{
    public async Task<OperationResult> HandleAsync(ForgotPasswordCommand command, CancellationToken cancellationToken = default)
    {
        var result = new OperationResult();

        try
        {
            var user = await userRepository.GetByEmailAsync(command.Email, cancellationToken);
            if (user is null || user.UserStatus != UserStatus.Active)
            {
                return result;
            }

            var resetCode = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            user.AssignPasswordResetCode(resetCode, DateTime.UtcNow.AddHours(2));
            if (!user.IsValid)
            {
                result.AddNotifications(user.Notifications);
                return result;
            }

            await userRepository.UpdateAsync(user, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await passwordResetEmailSender.SendPasswordResetCodeAsync(
                user.FullName,
                user.Email,
                resetCode,
                cancellationToken);

            return result;
        }
        catch (Exception)
        {
            result.SetAsInternalServerError();
            result.AddError("Unexpected error while requesting password reset.");
            return result;
        }
    }
}
