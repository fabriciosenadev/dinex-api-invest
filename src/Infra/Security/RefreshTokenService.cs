namespace DinExApi.Infra;

internal sealed class RefreshTokenService(IOptions<AppSettings> options) : IRefreshTokenService
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(14);

    public RefreshTokenResult Generate()
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(64);
        var rawToken = ToBase64Url(tokenBytes);
        var tokenHash = ComputeHash(rawToken);
        var expiresAtUtc = DateTime.UtcNow.Add(RefreshTokenLifetime);
        return new RefreshTokenResult(rawToken, tokenHash, expiresAtUtc);
    }

    public string ComputeHash(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new ArgumentException("Refresh token is required.", nameof(refreshToken));
        }

        var secret = options.Value.JwtSecret;
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new InvalidOperationException("AppSettings.JwtSecret is not configured.");
        }

        var input = Encoding.UTF8.GetBytes($"{refreshToken}:{secret}");
        var hash = SHA256.HashData(input);
        return Convert.ToHexString(hash);
    }

    private static string ToBase64Url(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
