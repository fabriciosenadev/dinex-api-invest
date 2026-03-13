
namespace DinExApi.Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddInfra(this IServiceCollection services, string databaseProvider, string? sqliteConnectionString, string? postgresConnectionString)
    {
        var provider = (databaseProvider ?? string.Empty).Trim().ToLowerInvariant();
        if (provider is "sqlite" or "postgres")
        {
            if (provider == "sqlite")
            {
                var sqliteConnection = string.IsNullOrWhiteSpace(sqliteConnectionString)
                    ? "Data Source=dinex.dev.db"
                    : sqliteConnectionString;
                services.AddDbContext<DinExDbContext>(options => options.UseSqlite(sqliteConnection));
            }
            else
            {
                var postgresConnection = string.IsNullOrWhiteSpace(postgresConnectionString)
                    ? throw new InvalidOperationException("ConnectionStrings:DinExPostgres is required when Database:Provider is postgres.")
                    : postgresConnectionString;
                services.AddDbContext<DinExDbContext>(options => options.UseNpgsql(postgresConnection));
            }

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IAssetDefinitionRepository, SqliteAssetDefinitionRepository>();
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

        throw new InvalidOperationException("Database:Provider must be sqlite or postgres.");
    }
}
