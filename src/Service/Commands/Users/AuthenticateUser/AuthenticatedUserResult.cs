namespace DinExApi.Service;

public sealed record AuthenticatedUserResult(
    Guid UserId,
    string FullName,
    string Email,
    string AccessToken,
    DateTime ExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);
