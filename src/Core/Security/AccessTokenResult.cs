namespace DinExApi.Core;

public sealed record AccessTokenResult(
    string AccessToken,
    DateTime ExpiresAtUtc);
