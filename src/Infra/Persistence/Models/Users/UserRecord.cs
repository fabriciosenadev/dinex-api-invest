namespace DinExApi.Infra;

public sealed class UserRecord
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int UserStatus { get; set; }
    public string? ActivationCode { get; set; }
    public DateTime? ActivationCodeExpiresAtUtc { get; set; }
    public int ActivationCodeFailedAttempts { get; set; }
    public string? PasswordResetCode { get; set; }
    public DateTime? PasswordResetCodeExpiresAtUtc { get; set; }
    public string? RefreshTokenHash { get; set; }
    public DateTime? RefreshTokenExpiresAtUtc { get; set; }
    public int LoginFailedAttempts { get; set; }
    public DateTime? LoginLockedUntilUtc { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
