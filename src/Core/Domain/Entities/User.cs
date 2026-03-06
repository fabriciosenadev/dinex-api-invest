namespace DinExApi.Core;

public sealed class User : Entity
{
    public string FullName { get; private set; }
    public string Email { get; private set; }
    public string Password { get; private set; }
    public UserStatus UserStatus { get; private set; }
    public string? ActivationCode { get; private set; }
    public DateTime? ActivationCodeExpiresAtUtc { get; private set; }
    public int ActivationCodeFailedAttempts { get; private set; }
    public string? PasswordResetCode { get; private set; }
    public DateTime? PasswordResetCodeExpiresAtUtc { get; private set; }
    public string? RefreshTokenHash { get; private set; }
    public DateTime? RefreshTokenExpiresAtUtc { get; private set; }
    public int LoginFailedAttempts { get; private set; }
    public DateTime? LoginLockedUntilUtc { get; private set; }

    public User(
        string fullName,
        string email,
        string password,
        UserStatus userStatus,
        string? activationCode,
        DateTime? activationCodeExpiresAtUtc,
        int activationCodeFailedAttempts,
        string? passwordResetCode,
        DateTime? passwordResetCodeExpiresAtUtc,
        string? refreshTokenHash,
        DateTime? refreshTokenExpiresAtUtc,
        int loginFailedAttempts,
        DateTime? loginLockedUntilUtc,
        DateTime createdAt,
        DateTime? updatedAt,
        DateTime? deletedAt,
        Guid? id = null)
    {
        Id = id ?? Guid.NewGuid();
        FullName = fullName.Trim();
        Email = email.Trim().ToLowerInvariant();
        Password = password;
        UserStatus = userStatus;
        ActivationCode = activationCode;
        ActivationCodeExpiresAtUtc = activationCodeExpiresAtUtc;
        ActivationCodeFailedAttempts = activationCodeFailedAttempts;
        PasswordResetCode = passwordResetCode;
        PasswordResetCodeExpiresAtUtc = passwordResetCodeExpiresAtUtc;
        RefreshTokenHash = refreshTokenHash;
        RefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc;
        LoginFailedAttempts = loginFailedAttempts;
        LoginLockedUntilUtc = loginLockedUntilUtc;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        DeletedAt = deletedAt;
    }

    public static User CreateUser(string fullName, string email, string password, string confirmPassword)
    {
        var newUser = new User(
            fullName: fullName,
            email: email.Trim().ToLowerInvariant(),
            password: string.Empty,
            userStatus: UserStatus.Inactive,
            activationCode: null,
            activationCodeExpiresAtUtc: null,
            activationCodeFailedAttempts: 0,
            passwordResetCode: null,
            passwordResetCodeExpiresAtUtc: null,
            refreshTokenHash: null,
            refreshTokenExpiresAtUtc: null,
            loginFailedAttempts: 0,
            loginLockedUntilUtc: null,
            createdAt: DateTime.UtcNow,
            updatedAt: null,
            deletedAt: null);

        newUser.AddNotifications(
            new Contract<Notification>()
                .Requires()
                .IsNotNullOrEmpty(newUser.FullName, "User.FullName", "Provide your full name")
                .IsGreaterOrEqualsThan(newUser.FullName?.Length ?? 0, 3, "User.FullName", "Provide your full name")
                .IsNotNullOrEmpty(newUser.Email, "User.Email", "Provide your best email")
                .IsEmail(newUser.Email, "User.Email", "Provide your best email"));

        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            newUser.AddNotification("User.Password", "Password must contain at least 8 characters");
        }

        if (password != confirmPassword)
        {
            newUser.AddNotification("User.ConfirmPassword", "Passwords must match");
        }

        if (newUser.IsValid)
        {
            newUser.Password = string.Empty;
        }

        return newUser;
    }

    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            AddNotification("User.PasswordHash", "Password hash is required.");
            return;
        }

        Password = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool HasValidActivationCode()
    {
        if (!ActivationCodeExpiresAtUtc.HasValue)
        {
            return false;
        }

        return ActivationCodeExpiresAtUtc.Value > DateTime.UtcNow;
    }

    public void AssignActivationCode(string activationCode)
    {
        ActivationCode = activationCode.Trim().ToUpperInvariant();
        ActivationCodeExpiresAtUtc = DateTime.UtcNow.AddHours(2);
        ActivationCodeFailedAttempts = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        ActivationCode = null;
        ActivationCodeExpiresAtUtc = null;
        ActivationCodeFailedAttempts = 0;
        UpdatedAt = DateTime.UtcNow;
        UserStatus = UserStatus.Active;
    }

    public bool HasExceededActivationAttempts(int maxAttempts)
    {
        return ActivationCodeFailedAttempts >= maxAttempts;
    }

    public bool RegisterActivationFailure(int maxAttempts)
    {
        ActivationCodeFailedAttempts += 1;
        UpdatedAt = DateTime.UtcNow;

        if (ActivationCodeFailedAttempts >= maxAttempts)
        {
            ActivationCode = null;
            ActivationCodeExpiresAtUtc = null;
            return true;
        }

        return false;
    }

    public void AssignPasswordResetCode(string passwordResetCode, DateTime expiresAtUtc)
    {
        if (string.IsNullOrWhiteSpace(passwordResetCode))
        {
            AddNotification("User.PasswordResetCode", "Password reset code is required.");
            return;
        }

        if (expiresAtUtc <= DateTime.UtcNow)
        {
            AddNotification("User.PasswordResetCode", "Password reset code expiration must be in the future.");
            return;
        }

        PasswordResetCode = passwordResetCode.Trim().ToUpperInvariant();
        PasswordResetCodeExpiresAtUtc = expiresAtUtc;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool HasValidPasswordResetCode(string passwordResetCode, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(PasswordResetCode) || string.IsNullOrWhiteSpace(passwordResetCode))
        {
            return false;
        }

        if (!string.Equals(PasswordResetCode, passwordResetCode.Trim().ToUpperInvariant(), StringComparison.Ordinal))
        {
            return false;
        }

        if (!PasswordResetCodeExpiresAtUtc.HasValue)
        {
            return false;
        }

        return PasswordResetCodeExpiresAtUtc.Value > nowUtc;
    }

    public void ClearPasswordResetCode()
    {
        PasswordResetCode = null;
        PasswordResetCodeExpiresAtUtc = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignRefreshToken(string refreshTokenHash, DateTime refreshTokenExpiresAtUtc)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenHash))
        {
            AddNotification("User.RefreshToken", "Refresh token hash is required.");
            return;
        }

        if (refreshTokenExpiresAtUtc <= DateTime.UtcNow)
        {
            AddNotification("User.RefreshToken", "Refresh token expiration must be in the future.");
            return;
        }

        RefreshTokenHash = refreshTokenHash;
        RefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool HasValidRefreshToken(string refreshTokenHash, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(RefreshTokenHash) || string.IsNullOrWhiteSpace(refreshTokenHash))
        {
            return false;
        }

        if (!string.Equals(RefreshTokenHash, refreshTokenHash, StringComparison.Ordinal))
        {
            return false;
        }

        if (!RefreshTokenExpiresAtUtc.HasValue)
        {
            return false;
        }

        return RefreshTokenExpiresAtUtc.Value > nowUtc;
    }

    public void RevokeRefreshToken()
    {
        RefreshTokenHash = null;
        RefreshTokenExpiresAtUtc = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsLoginLocked(DateTime nowUtc)
    {
        return LoginLockedUntilUtc.HasValue && LoginLockedUntilUtc.Value > nowUtc;
    }

    public bool RegisterFailedLogin(int maxAttempts, TimeSpan lockoutDuration, DateTime nowUtc)
    {
        LoginFailedAttempts += 1;
        UpdatedAt = nowUtc;

        if (LoginFailedAttempts >= maxAttempts)
        {
            LoginLockedUntilUtc = nowUtc.Add(lockoutDuration);
            LoginFailedAttempts = 0;
            return true;
        }

        return false;
    }

    public void RegisterSuccessfulLogin(DateTime nowUtc)
    {
        LoginFailedAttempts = 0;
        LoginLockedUntilUtc = null;
        UpdatedAt = nowUtc;
    }
}
