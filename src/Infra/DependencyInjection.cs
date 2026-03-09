
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
            services.AddScoped<ILedgerEntryRepository, SqliteLedgerEntryRepository>();
            services.AddScoped<ICorporateEventRepository, SqliteCorporateEventRepository>();
            services.AddScoped<ICorporateEventProcessor, SqliteCorporateEventProcessor>();
            services.AddScoped<IInvestmentPortfolioRebuilder, InvestmentPortfolioRebuilder>();
            services.AddScoped<IUserRepository, SqliteUserRepository>();
            services.AddScoped<IUserActivationEmailSender, UserActivationEmailSender>();
            services.AddScoped<IUserPasswordResetEmailSender, UserPasswordResetEmailSender>();
            services.AddScoped<IUserPasswordHasher, Argon2IdUserPasswordHasher>();
            services.AddScoped<IAccessTokenService, JwtAccessTokenService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IInvestmentSpreadsheetParser, B3InvestmentSpreadsheetParser>();
            services.AddScoped<IPortfolioPositionSpreadsheetParser, B3PortfolioPositionSpreadsheetParser>();
            services.AddScoped<IUnitOfWork, EfUnitOfWork>();
            return services;
        }

        services.AddSingleton<InMemoryDataStore>();
        services.AddScoped<IInvestmentOperationRepository, InMemoryInvestmentOperationRepository>();
        services.AddScoped<ILedgerEntryRepository, InMemoryLedgerEntryRepository>();
        services.AddScoped<ICorporateEventRepository, InMemoryCorporateEventRepository>();
        services.AddScoped<ICorporateEventProcessor, InMemoryCorporateEventProcessor>();
        services.AddScoped<IInvestmentPortfolioRebuilder, InvestmentPortfolioRebuilder>();
        services.AddScoped<IUserRepository, InMemoryUserRepository>();
        services.AddScoped<IUserActivationEmailSender, UserActivationEmailSender>();
        services.AddScoped<IUserPasswordResetEmailSender, UserPasswordResetEmailSender>();
        services.AddScoped<IUserPasswordHasher, Argon2IdUserPasswordHasher>();
        services.AddScoped<IAccessTokenService, JwtAccessTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IInvestmentSpreadsheetParser, B3InvestmentSpreadsheetParser>();
        services.AddScoped<IPortfolioPositionSpreadsheetParser, B3PortfolioPositionSpreadsheetParser>();
        services.AddScoped<IUnitOfWork, InMemoryUnitOfWork>();
        return services;
    }
}
