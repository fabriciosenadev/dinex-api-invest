namespace DinExApi.Infra;

public sealed class AppSettings
{
    public string JwtSecret { get; init; } = string.Empty;
    public string PasswordPepper { get; init; } = string.Empty;
    public string SmtpHost { get; init; } = string.Empty;
    public int SmtpPort { get; init; }
    public bool SmtpUseSsl { get; init; } = true;
    public string MailboxAddress { get; init; } = string.Empty;
    public string MailboxPassword { get; init; } = string.Empty;
    public string MailboxName { get; init; } = string.Empty;
    public string MailTemplateFolder { get; init; } = "Templates";
}
