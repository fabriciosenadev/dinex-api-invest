namespace DinExApi.Core;

public interface IUserRepository
{
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Default implementations keep old test doubles compiling while concrete repos override behavior.
    Task<bool> ExistsByRoleAsync(UserRole userRole, CancellationToken cancellationToken = default)
        => Task.FromResult(false);

    Task<IReadOnlyCollection<User>> ListAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyCollection<User>>(Array.Empty<User>());
}
