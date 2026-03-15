namespace DinExApi.Infra;

internal sealed class JwtAccessTokenService(IOptions<AppSettings> options) : IAccessTokenService
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(8);

    public AccessTokenResult Generate(User user)
    {
        var secret = options.Value.JwtSecret;
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new InvalidOperationException("AppSettings.JwtSecret is not configured.");
        }

        var now = DateTime.UtcNow;
        var expiresAtUtc = now.Add(TokenLifetime);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.UserRole.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        var token = new JwtSecurityToken(
            claims: claims,
            notBefore: now,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        return new AccessTokenResult(accessToken, expiresAtUtc);
    }
}
