namespace DinExApi.Tests;

public sealed class RefreshSessionCommandHandlerTests
{
    [Fact]
    public async Task Should_Return_Error_When_Refresh_Token_Is_Empty()
    {
        var handler = new RefreshSessionCommandHandler(
            new InMemoryUserRepositoryForTests(),
            new FakeAccessTokenServiceForTests(),
            new FakeRefreshTokenServiceForTests(),
            new SpyUnitOfWork());

        var result = await handler.HandleAsync(new RefreshSessionCommand(string.Empty));

        Assert.False(result.Succeeded);
        Assert.Contains("required", result.Errors.First());
    }

    [Fact]
    public async Task Should_Return_Error_When_User_Is_Not_Found_By_Refresh_Token()
    {
        var handler = new RefreshSessionCommandHandler(
            new InMemoryUserRepositoryForTests(),
            new FakeAccessTokenServiceForTests(),
            new FakeRefreshTokenServiceForTests(),
            new SpyUnitOfWork());

        var result = await handler.HandleAsync(new RefreshSessionCommand("refresh-token"));

        Assert.False(result.Succeeded);
        Assert.Contains("Invalid refresh token", result.Errors.First());
    }

    [Fact]
    public async Task Should_Return_Error_When_Account_Is_Not_Active()
    {
        var repository = new InMemoryUserRepositoryForTests();
        var user = User.CreateUser("Fabricio Sena", "fabricio@email.com", "Senha@123", "Senha@123");
        user.AssignRefreshToken("refresh-token-hash", DateTime.UtcNow.AddHours(1));
        await repository.AddAsync(user);
        var handler = new RefreshSessionCommandHandler(
            repository,
            new FakeAccessTokenServiceForTests(),
            new FakeRefreshTokenServiceForTests(),
            new SpyUnitOfWork());

        var result = await handler.HandleAsync(new RefreshSessionCommand("refresh-token"));

        Assert.False(result.Succeeded);
        Assert.Contains("not active", result.Errors.First());
    }

    [Fact]
    public async Task Should_Return_Error_When_Refresh_Token_Is_Expired()
    {
        var repository = new InMemoryUserRepositoryForTests();
        var user = new User(
            fullName: "Fabricio Sena",
            email: "fabricio@email.com",
            password: "hashed::Senha@123",
            userStatus: UserStatus.Active,
            activationCode: null,
            activationCodeExpiresAtUtc: null,
            activationCodeFailedAttempts: 0,
            passwordResetCode: null,
            passwordResetCodeExpiresAtUtc: null,
            refreshTokenHash: "refresh-token-hash",
            refreshTokenExpiresAtUtc: DateTime.UtcNow.AddMinutes(-1),
            loginFailedAttempts: 0,
            loginLockedUntilUtc: null,
            createdAt: DateTime.UtcNow,
            updatedAt: DateTime.UtcNow,
            deletedAt: null);
        await repository.AddAsync(user);
        var handler = new RefreshSessionCommandHandler(
            repository,
            new FakeAccessTokenServiceForTests(),
            new FakeRefreshTokenServiceForTests(),
            new SpyUnitOfWork());

        var result = await handler.HandleAsync(new RefreshSessionCommand("refresh-token"));

        Assert.False(result.Succeeded);
        Assert.Contains("expired", result.Errors.First());
    }

    [Fact]
    public async Task Should_Refresh_Session_When_Token_Is_Valid()
    {
        var repository = new InMemoryUserRepositoryForTests();
        var user = User.CreateUser("Fabricio Sena", "fabricio@email.com", "Senha@123", "Senha@123");
        user.Activate();
        user.AssignRefreshToken("refresh-token-hash", DateTime.UtcNow.AddHours(1));
        await repository.AddAsync(user);
        var unitOfWork = new SpyUnitOfWork();
        var handler = new RefreshSessionCommandHandler(
            repository,
            new FakeAccessTokenServiceForTests(),
            new FakeRefreshTokenServiceForTests(),
            unitOfWork);

        var result = await handler.HandleAsync(new RefreshSessionCommand("refresh-token"));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("access-token", result.Data.AccessToken);
        Assert.Equal("refresh-token", result.Data.RefreshToken);
        Assert.Equal(1, repository.UpdateCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Should_Return_Internal_Server_Error_When_Exception_Happens()
    {
        var handler = new RefreshSessionCommandHandler(
            new ThrowingUserRepository(),
            new FakeAccessTokenServiceForTests(),
            new FakeRefreshTokenServiceForTests(),
            new SpyUnitOfWork());

        var result = await handler.HandleAsync(new RefreshSessionCommand("refresh-token"));

        Assert.True(result.InternalServerError);
        Assert.Contains("Unexpected error", result.Errors.First());
    }

    private sealed class ThrowingUserRepository : IUserRepository
    {
        public Task AddAsync(User user, CancellationToken cancellationToken = default) => throw new InvalidOperationException("boom");
        public Task UpdateAsync(User user, CancellationToken cancellationToken = default) => throw new InvalidOperationException("boom");
        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) => throw new InvalidOperationException("boom");
        public Task<User?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken = default) => throw new InvalidOperationException("boom");
        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new InvalidOperationException("boom");
    }
}
