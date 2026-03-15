namespace DinExApi.Api.Configuration;

public static class ApplicationBuilderExtensions
{
    public static WebApplication ConfigureApiPipeline(this WebApplication app)
    {
        app.EnsureDatabaseMigrated();
        app.EnsureBootstrapAdmin();

        app.UseSerilogRequestLogging(options =>
        {
            options.GetLevel = (_, _, exception) =>
            {
                if (exception is not null)
                {
                    return LogEventLevel.Error;
                }

                return LogEventLevel.Information;
            };

            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("TraceId", Activity.Current?.Id ?? httpContext.TraceIdentifier);
                diagnosticContext.Set("UserId", httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous");
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("ClientIp", httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
            };
        });

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthentication();

        app.Use(async (context, next) =>
        {
            using (LogContext.PushProperty("TraceId", Activity.Current?.Id ?? context.TraceIdentifier))
            using (LogContext.PushProperty("UserId", context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous"))
            {
                await next();
            }
        });

        app.UseAuthorization();
        app.MapControllers();

        return app;
    }

    private static void EnsureDatabaseMigrated(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DinExDbContext>();
        var provider = app.Configuration["Database:Provider"] ?? "unknown";

        app.Logger.LogInformation("Applying database migrations on startup. Provider: {DatabaseProvider}", provider);

        try
        {
            dbContext.Database.Migrate();
            app.Logger.LogInformation("Database migrations applied successfully.");
        }
        catch (Exception exception)
        {
            app.Logger.LogCritical(exception, "Failed to apply database migrations on startup.");
            throw;
        }
    }

    private static void EnsureBootstrapAdmin(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var bootstrapService = scope.ServiceProvider.GetRequiredService<IAdminBootstrapService>();

        try
        {
            bootstrapService.EnsureAdminExistsAsync().GetAwaiter().GetResult();
        }
        catch (Exception exception)
        {
            app.Logger.LogCritical(exception, "Failed to ensure bootstrap admin user.");
            throw;
        }
    }
}
