namespace DinExApi.Infra;

internal sealed class UserPasswordResetEmailSender(IOptions<AppSettings> options) : IUserPasswordResetEmailSender
{
    private const string TemplateFileName = "UserPasswordResetCodeTemplate.html";

    public async Task SendPasswordResetCodeAsync(
        string fullName,
        string destinationEmail,
        string passwordResetCode,
        CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.SmtpHost)
            || settings.SmtpPort <= 0
            || string.IsNullOrWhiteSpace(settings.MailboxAddress)
            || string.IsNullOrWhiteSpace(settings.MailboxPassword))
        {
            throw new InvalidOperationException("SMTP settings are not configured.");
        }

        var body = await BuildEmailBodyAsync(settings, fullName, passwordResetCode, cancellationToken);

        using var message = new MailMessage
        {
            From = new MailAddress(settings.MailboxAddress, settings.MailboxName),
            Subject = "Codigo para redefinir sua senha - DinEx",
            Body = body,
            IsBodyHtml = true
        };
        message.To.Add(destinationEmail);

        using var smtpClient = new SmtpClient(settings.SmtpHost, settings.SmtpPort)
        {
            EnableSsl = settings.SmtpUseSsl,
            Credentials = new NetworkCredential(settings.MailboxAddress, settings.MailboxPassword)
        };

        await smtpClient.SendMailAsync(message, cancellationToken);
    }

    private static async Task<string> BuildEmailBodyAsync(
        AppSettings settings,
        string fullName,
        string passwordResetCode,
        CancellationToken cancellationToken)
    {
        var templateRoot = Path.Combine(AppContext.BaseDirectory, settings.MailTemplateFolder);
        var templatePath = Path.Combine(templateRoot, TemplateFileName);

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException("Password reset email template was not found.", templatePath);
        }

        var template = await File.ReadAllTextAsync(templatePath, cancellationToken);
        return template
            .Replace("{{full_name}}", fullName, StringComparison.Ordinal)
            .Replace("{{password_reset_code}}", passwordResetCode, StringComparison.Ordinal);
    }
}
