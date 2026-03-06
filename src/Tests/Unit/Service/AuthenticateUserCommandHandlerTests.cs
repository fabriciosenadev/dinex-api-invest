namespace DinExApi.Tests;

public sealed class AuthenticateUserCommandHandlerTests
{
    [Fact]
    public async Task Should_Lock_Login_After_Max_Failed_Attempts()
    {
        var repository = new FakeUserRepository();
        var unitOfWork = new InMemoryUnitOfWork();
        var hasher = new FakeUserPasswordHasher();
        var user = User.CreateUser("Fabricio Sena", "fabricio@email.com", "Senha@123", "Senha@123");
        user.Activate();
        user.SetPasswordHash(hasher.HashPassword("Senha@123"));
        await repository.AddAsync(user);

        var handler = new AuthenticateUserCommandHandler(
            repository,
            hasher,
            new FakeAccessTokenService(),
            new FakeRefreshTokenService(),
            unitOfWork);

        for (var i = 0; i < 5; i++)
        {
            var result = await handler.HandleAsync(new AuthenticateUserCommand(user.Email, "senha-invalida"));
            Assert.False(result.Succeeded);
        }

        var lockedAttempt = await handler.HandleAsync(new AuthenticateUserCommand(user.Email, "Senha@123"));

        Assert.False(lockedAttempt.Succeeded);
        Assert.Contains("Too many failed attempts", lockedAttempt.Errors.First());
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly List<User> _users = [];

        public Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            _users.Add(user);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
            => Task.FromResult(_users.FirstOrDefault(x => string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase)));

        public Task<User?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken = default)
            => Task.FromResult(_users.FirstOrDefault(x => string.Equals(x.RefreshTokenHash, refreshTokenHash, StringComparison.Ordinal)));

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_users.FirstOrDefault(x => x.Id == id));
    }

    private sealed class FakeUserPasswordHasher : IUserPasswordHasher
    {
        public string HashPassword(string password) => $"hashed::{password}";
        public bool VerifyPassword(string hashedPassword, string password) => hashedPassword == $"hashed::{password}";
    }

    private sealed class FakeAccessTokenService : IAccessTokenService
    {
        public AccessTokenResult Generate(User user) => new("access-token", DateTime.UtcNow.AddMinutes(15));
    }

    private sealed class FakeRefreshTokenService : IRefreshTokenService
    {
        public RefreshTokenResult Generate()
            => new("refresh-token", "refresh-token-hash", DateTime.UtcNow.AddDays(14));

        public string ComputeHash(string refreshToken) => "refresh-token-hash";
    }
}
