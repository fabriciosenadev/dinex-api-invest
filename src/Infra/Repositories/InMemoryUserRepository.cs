namespace DinExApi.Infra;

internal sealed class InMemoryUserRepository(InMemoryDataStore dataStore) : IUserRepository
{
    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        dataStore.AddUser(user);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => Task.FromResult(dataStore.FindUserByEmail(email));

    public Task<User?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken = default)
        => Task.FromResult(dataStore.FindUserByRefreshTokenHash(refreshTokenHash));

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(dataStore.FindUserById(id));

    public Task<bool> ExistsByRoleAsync(UserRole userRole, CancellationToken cancellationToken = default)
        => Task.FromResult(dataStore.ExistsUserByRole(userRole));

    public Task<IReadOnlyCollection<User>> ListAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(dataStore.SnapshotUsers());
}
