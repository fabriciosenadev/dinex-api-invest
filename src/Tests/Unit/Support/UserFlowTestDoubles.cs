namespace DinExApi.Tests;

internal sealed class SpyUnitOfWork : IUnitOfWork
{
    public int SaveChangesCallCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount += 1;
        return Task.FromResult(1);
    }
}

internal sealed class InMemoryUserRepositoryForTests : IUserRepository
{
    private readonly List<User> _users = [];

    public int AddCallCount { get; private set; }
    public int UpdateCallCount { get; private set; }

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        AddCallCount += 1;
        _users.Add(user);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        UpdateCallCount += 1;
        return Task.CompletedTask;
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.FirstOrDefault(x =>
            string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase)));
    }

    public Task<User?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.FirstOrDefault(x =>
            string.Equals(x.RefreshTokenHash, refreshTokenHash, StringComparison.Ordinal)));
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_users.FirstOrDefault(x => x.Id == id));
    }
}

internal sealed class FakeUserPasswordHasherForTests : IUserPasswordHasher
{
    public string HashPassword(string password) => $"hashed::{password}";

    public bool VerifyPassword(string hashedPassword, string password)
    {
        return hashedPassword == $"hashed::{password}";
    }
}

internal sealed class SpyActivationEmailSender : IUserActivationEmailSender
{
    public int SendCallCount { get; private set; }
    public string? LastCode { get; private set; }

    public Task SendActivationCodeAsync(
        string fullName,
        string destinationEmail,
        string activationCode,
        CancellationToken cancellationToken = default)
    {
        SendCallCount += 1;
        LastCode = activationCode;
        return Task.CompletedTask;
    }
}

internal sealed class SpyPasswordResetEmailSender : IUserPasswordResetEmailSender
{
    public int SendCallCount { get; private set; }

    public Task SendPasswordResetCodeAsync(
        string fullName,
        string destinationEmail,
        string passwordResetCode,
        CancellationToken cancellationToken = default)
    {
        SendCallCount += 1;
        return Task.CompletedTask;
    }
}

internal sealed class FakeAccessTokenServiceForTests : IAccessTokenService
{
    public AccessTokenResult Generate(User user)
    {
        return new AccessTokenResult("access-token", DateTime.UtcNow.AddMinutes(15));
    }
}

internal sealed class FakeRefreshTokenServiceForTests : IRefreshTokenService
{
    public RefreshTokenResult Generate()
    {
        return new RefreshTokenResult("refresh-token", "refresh-token-hash-new", DateTime.UtcNow.AddDays(14));
    }

    public string ComputeHash(string refreshToken)
    {
        return refreshToken == "refresh-token" ? "refresh-token-hash" : "invalid";
    }
}
