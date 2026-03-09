namespace DinExApi.Tests;

public sealed class ActivateUserCommandHandlerTests
{
    [Fact]
    public async Task Should_Activate_User_And_Clear_Activation_Code()
    {
        var repository = new FakeUserRepository();
        var unitOfWork = new SpyUnitOfWork();
        var registerHandler = new RegisterUserCommandHandler(
            repository,
            new FakeUserPasswordHasher(),
            new FakeActivationEmailSender(),
            unitOfWork);
        var activateHandler = new ActivateUserCommandHandler(repository, unitOfWork);

        var registerResult = await registerHandler.HandleAsync(new RegisterUserCommand(
            "Fabricio Sena",
            "fabricio@email.com",
            "Senha@123",
            "Senha@123"));

        var user = await repository.GetByIdAsync(registerResult.Data);
        var activationCode = user!.ActivationCode!;

        var activationResult = await activateHandler.HandleAsync(
            new ActivateUserCommand(user.Email, activationCode));

        var updatedUser = await repository.GetByIdAsync(user.Id);

        Assert.True(registerResult.Succeeded);
        Assert.True(activationResult.Succeeded);
        Assert.Equal(UserStatus.Active, updatedUser!.UserStatus);
        Assert.Null(updatedUser.ActivationCode);
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

    private sealed class FakeActivationEmailSender : IUserActivationEmailSender
    {
        public Task SendActivationCodeAsync(
            string fullName,
            string destinationEmail,
            string activationCode,
            CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeUserPasswordHasher : IUserPasswordHasher
    {
        public string HashPassword(string password) => $"hashed::{password}";
        public bool VerifyPassword(string hashedPassword, string password) => hashedPassword == $"hashed::{password}";
    }
}
