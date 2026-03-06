namespace DinExApi.Core;

public interface IUserPasswordResetEmailSender
{
    Task SendPasswordResetCodeAsync(
        string fullName,
        string destinationEmail,
        string passwordResetCode,
        CancellationToken cancellationToken = default);
}
