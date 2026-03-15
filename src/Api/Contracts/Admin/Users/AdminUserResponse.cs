namespace DinExApi.Api.Contracts.Admin.Users;

public sealed record AdminUserResponse(
    Guid UserId,
    string FullName,
    string Email,
    string UserStatus,
    string UserRole,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
