namespace DinExApi.Core;

public sealed record RefreshTokenResult(
    string Token,
    string TokenHash,
    DateTime ExpiresAtUtc);
