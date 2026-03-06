namespace DinExApi.Tests;

public sealed class GetCurrentUserQueryHandlerTests
{
    [Fact]
    public async Task Should_Return_Current_User_When_User_Exists()
    {
        var repository = new FakeUserRepository();
        var user = User.CreateUser("Fabricio Sena", "fabricio@email.com", "Senha@123", "Senha@123");
        await repository.AddAsync(user);

        var handler = new GetCurrentUserQueryHandler(repository);
        var result = await handler.HandleAsync(new GetCurrentUserQuery(user.Id));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal(user.Id, result.Data!.UserId);
        Assert.Equal(user.Email, result.Data.Email);
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
