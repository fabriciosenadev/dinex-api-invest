namespace DinExApi.Tests;

public sealed class ActivationCodeFlowTests
{
    [Fact]
    public async Task Should_Block_Activation_After_Max_Attempts_And_Require_Resend()
    {
        var repository = new FakeUserRepository();
        var unitOfWork = new InMemoryUnitOfWork();
        var registerHandler = new RegisterUserCommandHandler(
            repository,
            new FakeUserPasswordHasher(),
            new FakeActivationEmailSender(),
            unitOfWork);
        var activateHandler = new ActivateUserCommandHandler(repository, unitOfWork);
        var resendHandler = new ResendActivationCodeCommandHandler(repository, new FakeActivationEmailSender(), unitOfWork);

        var registerResult = await registerHandler.HandleAsync(new RegisterUserCommand(
            "Fabricio Sena",
            "fabricio@email.com",
            "Senha@123",
            "Senha@123"));
        Assert.True(registerResult.Succeeded);

        var user = await repository.GetByIdAsync(registerResult.Data);
        Assert.NotNull(user);

        for (var i = 0; i < 5; i++)
        {
            var failed = await activateHandler.HandleAsync(new ActivateUserCommand(user!.Email, "INVALIDO"));
            Assert.False(failed.Succeeded);
        }

        var blockedUser = await repository.GetByIdAsync(user!.Id);
        Assert.NotNull(blockedUser);
        Assert.Null(blockedUser!.ActivationCode);

        var resend = await resendHandler.HandleAsync(new ResendActivationCodeCommand(blockedUser.Email));
        Assert.True(resend.Succeeded);

        var resentUser = await repository.GetByIdAsync(user.Id);
        Assert.NotNull(resentUser);
        Assert.False(string.IsNullOrWhiteSpace(resentUser!.ActivationCode));
        Assert.Equal(0, resentUser.ActivationCodeFailedAttempts);
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
