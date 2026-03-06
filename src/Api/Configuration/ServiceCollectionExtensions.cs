
namespace DinExApi.Api.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiDependencies(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        services.AddControllers();

        var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>() ?? new AppSettings();
        if (string.IsNullOrWhiteSpace(appSettings.JwtSecret))
        {
            throw new InvalidOperationException("AppSettings.JwtSecret is required to configure JWT authentication.");
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.JwtSecret));
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });
        services.AddAuthorization();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerWithBearer();
        services.AddService();
        services.AddInfra(
            environment.IsDevelopment(),
            configuration.GetConnectionString("DinExSqlite"));

        return services;
    }

    private static IServiceCollection AddSwaggerWithBearer(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: Bearer {token}",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer", hostDocument: document, externalResource: null),
                    []
                }
            });
        });

        return services;
    }
}
