namespace DinExApi.Tests;

public sealed class ForgotPasswordCommandHandlerTests
{
    [Fact]
    public async Task Should_Return_Success_When_User_Does_Not_Exist()
    {
        var repository = new InMemoryUserRepositoryForTests();
        var sender = new SpyPasswordResetEmailSender();
        var unitOfWork = new SpyUnitOfWork();
        var handler = new ForgotPasswordCommandHandler(repository, sender, unitOfWork);

        var result = await handler.HandleAsync(new ForgotPasswordCommand("naoexiste@email.com"));

        Assert.True(result.Succeeded);
        Assert.Equal(0, repository.UpdateCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
        Assert.Equal(0, sender.SendCallCount);
    }

    [Fact]
    public async Task Should_Return_Success_Without_Sending_Email_When_User_Is_Inactive()
    {
        var repository = new InMemoryUserRepositoryForTests();
        var user = User.CreateUser("Fabricio Sena", "fabricio@email.com", "Senha@123", "Senha@123");
        await repository.AddAsync(user);
        var sender = new SpyPasswordResetEmailSender();
        var unitOfWork = new SpyUnitOfWork();
        var handler = new ForgotPasswordCommandHandler(repository, sender, unitOfWork);

        var result = await handler.HandleAsync(new ForgotPasswordCommand(user.Email));

        Assert.True(result.Succeeded);
        Assert.Equal(0, repository.UpdateCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
        Assert.Equal(0, sender.SendCallCount);
    }

    [Fact]
    public async Task Should_Assign_Reset_Code_And_Send_Email_When_User_Is_Active()
    {
        var repository = new InMemoryUserRepositoryForTests();
        var user = User.CreateUser("Fabricio Sena", "fabricio@email.com", "Senha@123", "Senha@123");
        user.Activate();
        await repository.AddAsync(user);
        var sender = new SpyPasswordResetEmailSender();
        var unitOfWork = new SpyUnitOfWork();
        var handler = new ForgotPasswordCommandHandler(repository, sender, unitOfWork);

        var result = await handler.HandleAsync(new ForgotPasswordCommand(user.Email));

        Assert.True(result.Succeeded);
        Assert.Equal(1, repository.UpdateCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Equal(1, sender.SendCallCount);
        var updatedUser = await repository.GetByIdAsync(user.Id);
        Assert.NotNull(updatedUser!.PasswordResetCode);
        Assert.True(updatedUser.PasswordResetCodeExpiresAtUtc > DateTime.UtcNow);
    }

    [Fact]
    public async Task Should_Return_Internal_Server_Error_When_Repository_Throws()
    {
        var handler = new ForgotPasswordCommandHandler(
            new ThrowingUserRepository(),
            new SpyPasswordResetEmailSender(),
            new SpyUnitOfWork());

        var result = await handler.HandleAsync(new ForgotPasswordCommand("fabricio@email.com"));

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
