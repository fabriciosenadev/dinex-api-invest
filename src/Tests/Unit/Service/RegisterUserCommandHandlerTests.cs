namespace DinExApi.Tests;

public sealed class RegisterUserCommandHandlerTests
{
    [Fact]
    public async Task Should_Register_User_And_Send_Activation_Email()
    {
        var repository = new InMemoryUserRepositoryForTests();
        var hasher = new FakeUserPasswordHasherForTests();
        var emailSender = new SpyActivationEmailSender();
        var unitOfWork = new SpyUnitOfWork();
        var handler = new RegisterUserCommandHandler(repository, hasher, emailSender, unitOfWork);

        var result = await handler.HandleAsync(new RegisterUserCommand(
            "Fabricio Sena",
            "fabricio@email.com",
            "Senha@123",
            "Senha@123"));

        Assert.True(result.Succeeded);
        Assert.NotEqual(Guid.Empty, result.Data);
        Assert.Equal(1, repository.AddCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.Equal(1, emailSender.SendCallCount);
    }

    [Fact]
    public async Task Should_Return_Error_When_Email_Already_Exists()
    {
        var repository = new InMemoryUserRepositoryForTests();
        var hasher = new FakeUserPasswordHasherForTests();
        var emailSender = new SpyActivationEmailSender();
        var unitOfWork = new SpyUnitOfWork();
        var existing = User.CreateUser("Fabricio Sena", "fabricio@email.com", "Senha@123", "Senha@123");
        existing.SetPasswordHash(hasher.HashPassword("Senha@123"));
        await repository.AddAsync(existing);
        var handler = new RegisterUserCommandHandler(repository, hasher, emailSender, unitOfWork);

        var result = await handler.HandleAsync(new RegisterUserCommand(
            "Fabricio Sena",
            "fabricio@email.com",
            "Senha@123",
            "Senha@123"));

        Assert.False(result.Succeeded);
        Assert.Contains("already exists", result.Errors.First());
    }

    [Fact]
    public async Task Should_Return_Validation_Errors_When_Command_Data_Is_Invalid()
    {
        var handler = new RegisterUserCommandHandler(
            new InMemoryUserRepositoryForTests(),
            new FakeUserPasswordHasherForTests(),
            new SpyActivationEmailSender(),
            new SpyUnitOfWork());

        var result = await handler.HandleAsync(new RegisterUserCommand(
            "Fa",
            "email-invalido",
            "123",
            "456"));

        Assert.False(result.Succeeded);
        Assert.True(result.Errors.Any());
    }

    [Fact]
    public async Task Should_Return_Internal_Server_Error_When_Exception_Happens()
    {
        var handler = new RegisterUserCommandHandler(
            new ThrowingUserRepository(),
            new FakeUserPasswordHasherForTests(),
            new SpyActivationEmailSender(),
            new SpyUnitOfWork());

        var result = await handler.HandleAsync(new RegisterUserCommand(
            "Fabricio Sena",
            "fabricio@email.com",
            "Senha@123",
            "Senha@123"));

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
