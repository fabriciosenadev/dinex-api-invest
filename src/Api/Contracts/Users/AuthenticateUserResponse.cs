namespace DinExApi.Api.Contracts.Users;

public sealed record AuthenticateUserResponse(
    Guid UserId,
    string FullName,
    string Email,
    string AccessToken,
    DateTime ExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);
