namespace DinExApi.Api.Configuration;

public static class ApplicationBuilderExtensions
{
    public static WebApplication ConfigureApiPipeline(this WebApplication app)
    {
        app.EnsureDatabaseMigrated();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }

    private static void EnsureDatabaseMigrated(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DinExDbContext>();
        dbContext.Database.Migrate();
    }
}
