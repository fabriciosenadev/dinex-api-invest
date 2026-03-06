namespace DinExApi.Infra;

internal sealed class UserActivationEmailSender(IOptions<AppSettings> options) : IUserActivationEmailSender
{
    private const string TemplateFileName = "UserActivationCodeTemplate.html";

    public async Task SendActivationCodeAsync(
        string fullName,
        string destinationEmail,
        string activationCode,
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

        var body = await BuildEmailBodyAsync(settings, fullName, activationCode, cancellationToken);

        using var message = new MailMessage
        {
            From = new MailAddress(settings.MailboxAddress, settings.MailboxName),
            Subject = "Codigo de ativacao da conta DinEx",
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
        string activationCode,
        CancellationToken cancellationToken)
    {
        var templateRoot = Path.Combine(AppContext.BaseDirectory, settings.MailTemplateFolder);
        var templatePath = Path.Combine(templateRoot, TemplateFileName);

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException("Activation email template was not found.", templatePath);
        }

        var template = await File.ReadAllTextAsync(templatePath, cancellationToken);
        return template
            .Replace("{{full_name}}", fullName, StringComparison.Ordinal)
            .Replace("{{activation_code}}", activationCode, StringComparison.Ordinal);
    }
}
