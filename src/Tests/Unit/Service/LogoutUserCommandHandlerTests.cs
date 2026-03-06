namespace DinExApi.Tests;

public sealed class LogoutUserCommandHandlerTests
{
    [Fact]
    public async Task Should_Revoke_Refresh_Token_On_Logout()
    {
        var repository = new FakeUserRepository();
        var unitOfWork = new InMemoryUnitOfWork();
        var user = User.CreateUser("Fabricio Sena", "fabricio@email.com", "Senha@123", "Senha@123");
        user.Activate();
        user.AssignRefreshToken("hash-token", DateTime.UtcNow.AddHours(1));
        await repository.AddAsync(user);

        var handler = new LogoutUserCommandHandler(repository, unitOfWork);

        var result = await handler.HandleAsync(new LogoutUserCommand(user.Id));
        var updatedUser = await repository.GetByIdAsync(user.Id);

        Assert.True(result.Succeeded);
        Assert.NotNull(updatedUser);
        Assert.Null(updatedUser!.RefreshTokenHash);
        Assert.Null(updatedUser.RefreshTokenExpiresAtUtc);
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
}
