namespace api.Services.DatabaseService;

public class DatabaseInitializerService(IServiceProvider serviceProvider, ILogger<DatabaseInitializerService> logger)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting Database Initialization Task...");

        using var scope = serviceProvider.CreateScope();
        var dbService = scope.ServiceProvider.GetRequiredService<IDbService>();

        // This will now be traced and logged via OpenTelemetry
        if (!DatabaseInitializer.IsDatabaseInitialized(dbService))
        {
            logger.LogWarning("🗄️ Database not found. Running first-time setup...");
            DatabaseInitializer.Initialize(dbService);
        }
        else
        {
            logger.LogInformation("✅ Database already initialized. Skipping setup.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
