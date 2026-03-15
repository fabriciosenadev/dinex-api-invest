namespace DinExApi.Service;

public sealed class AdminBootstrapService(
    IUserRepository userRepository,
    IUserPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    IOptions<BootstrapAdminSettings> options,
    ILogger<AdminBootstrapService> logger) : IAdminBootstrapService
{
    public async Task EnsureAdminExistsAsync(CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            return;
        }

        if (await userRepository.ExistsByRoleAsync(UserRole.Admin, cancellationToken))
        {
            logger.LogInformation("Bootstrap admin skipped because an admin user already exists.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.FullName)
            || string.IsNullOrWhiteSpace(settings.Email)
            || string.IsNullOrWhiteSpace(settings.Password))
        {
            throw new InvalidOperationException("BootstrapAdmin settings are incomplete.");
        }

        var existingUser = await userRepository.GetByEmailAsync(settings.Email, cancellationToken);
        if (existingUser is not null)
        {
            existingUser.PromoteToAdmin(activateUser: true);
            await userRepository.UpdateAsync(existingUser, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Bootstrap admin promoted existing user {Email} to Admin and activated the account.", existingUser.Email);
            return;
        }

        var adminUser = User.CreateAdmin(settings.FullName, settings.Email);
        adminUser.SetPasswordHash(passwordHasher.HashPassword(settings.Password));
        if (!adminUser.IsValid)
        {
            throw new InvalidOperationException(string.Join(" | ", adminUser.Notifications.Select(x => x.Message)));
        }

        await userRepository.AddAsync(adminUser, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Bootstrap admin user created for {Email}.", adminUser.Email);
    }
}
