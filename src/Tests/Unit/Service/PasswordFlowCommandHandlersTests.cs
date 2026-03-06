namespace DinExApi.Tests;

public sealed class PasswordFlowCommandHandlersTests
{
    [Fact]
    public async Task Should_Reset_Password_Using_Valid_Code()
    {
        var repository = new FakeUserRepository();
        var unitOfWork = new InMemoryUnitOfWork();
        var hasher = new FakeUserPasswordHasher();
        var user = User.CreateUser("Fabricio Sena", "fabricio@email.com", "Senha@123", "Senha@123");
        user.Activate();
        user.SetPasswordHash(hasher.HashPassword("Senha@123"));
        user.AssignPasswordResetCode("ABC12345", DateTime.UtcNow.AddHours(1));
        await repository.AddAsync(user);

        var handler = new ResetPasswordCommandHandler(repository, hasher, unitOfWork);

        var result = await handler.HandleAsync(new ResetPasswordCommand(
            user.Email,
            "ABC12345",
            "NovaSenha@123",
            "NovaSenha@123"));

        var updatedUser = await repository.GetByIdAsync(user.Id);

        Assert.True(result.Succeeded);
        Assert.NotNull(updatedUser);
        Assert.True(hasher.VerifyPassword(updatedUser!.Password, "NovaSenha@123"));
        Assert.Null(updatedUser.PasswordResetCode);
        Assert.Null(updatedUser.PasswordResetCodeExpiresAtUtc);
    }

    [Fact]
    public async Task Should_Change_Password_When_Current_Password_Is_Valid()
    {
        var repository = new FakeUserRepository();
        var unitOfWork = new InMemoryUnitOfWork();
        var hasher = new FakeUserPasswordHasher();
        var user = User.CreateUser("Fabricio Sena", "fabricio@email.com", "Senha@123", "Senha@123");
        user.Activate();
        user.SetPasswordHash(hasher.HashPassword("Senha@123"));
        await repository.AddAsync(user);

        var handler = new ChangePasswordCommandHandler(repository, hasher, unitOfWork);

        var result = await handler.HandleAsync(new ChangePasswordCommand(
            user.Id,
            "Senha@123",
            "NovaSenha@123",
            "NovaSenha@123"));

        var updatedUser = await repository.GetByIdAsync(user.Id);

        Assert.True(result.Succeeded);
        Assert.NotNull(updatedUser);
        Assert.True(hasher.VerifyPassword(updatedUser!.Password, "NovaSenha@123"));
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
}
