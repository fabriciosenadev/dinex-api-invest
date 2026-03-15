namespace DinExApi.Infra;

internal sealed class SqliteUserRepository(IRepository<UserRecord> repository) : IUserRepository
{
    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await repository.AddAsync(user.ToRecord(), cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var record = await repository.Query()
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);

        return record?.ToEntity();
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await repository.Query()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return record?.ToEntity();
    }

    public async Task<User?> GetByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken = default)
    {
        var record = await repository.Query()
            .FirstOrDefaultAsync(x => x.RefreshTokenHash == refreshTokenHash, cancellationToken);

        return record?.ToEntity();
    }

    public async Task<bool> ExistsByRoleAsync(UserRole userRole, CancellationToken cancellationToken = default)
    {
        return await repository.Query()
            .AnyAsync(x => x.UserRole == (int)userRole, cancellationToken);
    }

    public async Task<IReadOnlyCollection<User>> ListAsync(CancellationToken cancellationToken = default)
    {
        var records = await repository.Query()
            .OrderBy(x => x.FullName)
            .ToListAsync(cancellationToken);

        return records.Select(x => x.ToEntity()).ToArray();
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        var trackedRecord = await repository.Query(asNoTracking: false)
            .FirstOrDefaultAsync(x => x.Id == user.Id, cancellationToken);

        if (trackedRecord is null)
        {
            return;
        }

        trackedRecord.UpdateFromEntity(user);
        repository.Update(trackedRecord);
    }
}
