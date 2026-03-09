namespace DinExApi.Tests;

public sealed class ResendActivationCodeCommandHandlerTests
{
    [Fact]
    public async Task Should_Return_NotFound_When_User_Does_Not_Exist()
    {
        var handler = new ResendActivationCodeCommandHandler(
            new InMemoryUserRepositoryForTests(),
            new SpyActivationEmailSender(),
            new SpyUnitOfWork());

        var result = await handler.HandleAsync(new ResendActivationCodeCommand("naoexiste@email.com"));

        Assert.True(result.IsNotFound);
        Assert.Contains("User not found", result.Errors.First());
    }

    [Fact]
    public async Task Should_Return_Error_When_User_Already_Active()
    {
        var repository = new InMemoryUserRepositoryForTests();
        var user = User.CreateUser("Fabricio Sena", "fabricio@email.com", "Senha@123", "Senha@123");
        user.Activate();
        await repository.AddAsync(user);
        var handler = new ResendActivationCodeCommandHandler(repository, new SpyActivationEmailSender(), new SpyUnitOfWork());

        var result = await handler.HandleAsync(new ResendActivationCodeCommand(user.Email));

        Assert.False(result.Succeeded);
        Assert.Contains("already active", result.Errors.First());
    }

    [Fact]
    public async Task Should_Resend_Activation_Code_When_User_Is_Inactive()
    {
        var repository = new InMemoryUserRepositoryForTests();
        var user = User.CreateUser("Fabricio Sena", "fabricio@email.com", "Senha@123", "Senha@123");
        user.SetPasswordHash("hashed");
        await repository.AddAsync(user);
        var sender = new SpyActivationEmailSender();
        var unitOfWork = new SpyUnitOfWork();
        var handler = new ResendActivationCodeCommandHandler(repository, sender, unitOfWork);

        var result = await handler.HandleAsync(new ResendActivationCodeCommand(user.Email));

        Assert.True(result.Succeeded);
        Assert.Equal(1, repository.UpdateCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Equal(1, sender.SendCallCount);
    }

    [Fact]
    public async Task Should_Return_Internal_Server_Error_When_Exception_Happens()
    {
        var handler = new ResendActivationCodeCommandHandler(
            new ThrowingUserRepository(),
            new SpyActivationEmailSender(),
            new SpyUnitOfWork());

        var result = await handler.HandleAsync(new ResendActivationCodeCommand("fabricio@email.com"));

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
