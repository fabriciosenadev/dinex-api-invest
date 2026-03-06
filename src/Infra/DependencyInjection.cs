
namespace DinExApi.Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddInfra(this IServiceCollection services, bool useSqliteInDevelopment, string? connectionString)
    {
        if (useSqliteInDevelopment)
        {
            var sqliteConnection = string.IsNullOrWhiteSpace(connectionString)
                ? "Data Source=dinex.dev.db"
                : connectionString;

            services.AddDbContext<DinExDbContext>(options => options.UseSqlite(sqliteConnection));
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IInvestmentOperationRepository, SqliteInvestmentOperationRepository>();
            services.AddScoped<IUserRepository, SqliteUserRepository>();
            services.AddScoped<IUserActivationEmailSender, UserActivationEmailSender>();
            services.AddScoped<IUserPasswordResetEmailSender, UserPasswordResetEmailSender>();
            services.AddScoped<IUserPasswordHasher, Argon2IdUserPasswordHasher>();
            services.AddScoped<IAccessTokenService, JwtAccessTokenService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IUnitOfWork, EfUnitOfWork>();
            return services;
        }

        services.AddSingleton<InMemoryDataStore>();
        services.AddScoped<IInvestmentOperationRepository, InMemoryInvestmentOperationRepository>();
        services.AddScoped<IUserRepository, InMemoryUserRepository>();
        services.AddScoped<IUserActivationEmailSender, UserActivationEmailSender>();
        services.AddScoped<IUserPasswordResetEmailSender, UserPasswordResetEmailSender>();
        services.AddScoped<IUserPasswordHasher, Argon2IdUserPasswordHasher>();
        services.AddScoped<IAccessTokenService, JwtAccessTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IUnitOfWork, InMemoryUnitOfWork>();
        return services;
    }
}
