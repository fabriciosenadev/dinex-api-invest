namespace DinExApi.Api.Contracts.Users;

public sealed record CurrentUserResponse(
    Guid UserId,
    string FullName,
    string Email,
    string UserStatus,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
