namespace DinExApi.Core;

public interface IUserActivationEmailSender
{
    Task SendActivationCodeAsync(
        string fullName,
        string destinationEmail,
        string activationCode,
        CancellationToken cancellationToken = default);
}
