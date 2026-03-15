namespace DinExApi.Service;

public sealed record AdminUserItem(
    Guid UserId,
    string FullName,
    string Email,
    UserStatus UserStatus,
    UserRole UserRole,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
