namespace DinExApi.Infra;

internal static class UserMappings
{
    public static UserRecord ToRecord(this User entity)
    {
        return new UserRecord
        {
            Id = entity.Id,
            FullName = entity.FullName,
            Email = entity.Email,
            Password = entity.Password,
            UserStatus = (int)entity.UserStatus,
            UserRole = (int)entity.UserRole,
            ActivationCode = entity.ActivationCode,
            ActivationCodeExpiresAtUtc = entity.ActivationCodeExpiresAtUtc,
            ActivationCodeFailedAttempts = entity.ActivationCodeFailedAttempts,
            PasswordResetCode = entity.PasswordResetCode,
            PasswordResetCodeExpiresAtUtc = entity.PasswordResetCodeExpiresAtUtc,
            RefreshTokenHash = entity.RefreshTokenHash,
            RefreshTokenExpiresAtUtc = entity.RefreshTokenExpiresAtUtc,
            LoginFailedAttempts = entity.LoginFailedAttempts,
            LoginLockedUntilUtc = entity.LoginLockedUntilUtc,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            DeletedAt = entity.DeletedAt
        };
    }

    public static User ToEntity(this UserRecord record)
    {
        return new User(
            fullName: record.FullName,
            email: record.Email,
            password: record.Password,
            userStatus: (UserStatus)record.UserStatus,
            activationCode: record.ActivationCode,
            activationCodeExpiresAtUtc: record.ActivationCodeExpiresAtUtc,
            activationCodeFailedAttempts: record.ActivationCodeFailedAttempts,
            passwordResetCode: record.PasswordResetCode,
            passwordResetCodeExpiresAtUtc: record.PasswordResetCodeExpiresAtUtc,
            refreshTokenHash: record.RefreshTokenHash,
            refreshTokenExpiresAtUtc: record.RefreshTokenExpiresAtUtc,
            loginFailedAttempts: record.LoginFailedAttempts,
            loginLockedUntilUtc: record.LoginLockedUntilUtc,
            createdAt: record.CreatedAt,
            updatedAt: record.UpdatedAt,
            deletedAt: record.DeletedAt,
            id: record.Id,
            userRole: (UserRole)record.UserRole);
    }

    public static void UpdateFromEntity(this UserRecord record, User entity)
    {
        record.FullName = entity.FullName;
        record.Email = entity.Email;
        record.Password = entity.Password;
        record.UserStatus = (int)entity.UserStatus;
        record.UserRole = (int)entity.UserRole;
        record.ActivationCode = entity.ActivationCode;
        record.ActivationCodeExpiresAtUtc = entity.ActivationCodeExpiresAtUtc;
        record.ActivationCodeFailedAttempts = entity.ActivationCodeFailedAttempts;
        record.PasswordResetCode = entity.PasswordResetCode;
        record.PasswordResetCodeExpiresAtUtc = entity.PasswordResetCodeExpiresAtUtc;
        record.RefreshTokenHash = entity.RefreshTokenHash;
        record.RefreshTokenExpiresAtUtc = entity.RefreshTokenExpiresAtUtc;
        record.LoginFailedAttempts = entity.LoginFailedAttempts;
        record.LoginLockedUntilUtc = entity.LoginLockedUntilUtc;
        record.CreatedAt = entity.CreatedAt;
        record.UpdatedAt = entity.UpdatedAt;
        record.DeletedAt = entity.DeletedAt;
    }
}
