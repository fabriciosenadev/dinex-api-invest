namespace DinExApi.Service;

public sealed record CurrentUserItem(
    Guid UserId,
    string FullName,
    string Email,
    UserStatus UserStatus,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
