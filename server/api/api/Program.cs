using api.Services;
using System.Text.Json;
using api.Services.DatabaseService;
using LinqToDB;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Services

// AppSettings
var appSettings = new AppSettings();
builder.Configuration.GetSection("AppSettings").Bind(appSettings);
builder.Services.AddSingleton(appSettings);

// Manual Registration for PostgresSQL
builder.Services.AddScoped<IDbService>(provider => 
{
    // Get the settings we registered as a Singleton earlier
    var settings = provider.GetRequiredService<AppSettings>();
    
    // Get the connection string from your nested settings object
    var connectionString = settings.DbConn.PostgresConn.ConnectionString;

    // Configure LinqToDB for PostgresSQL
    var options = new DataOptions()
        .UsePostgreSQL(connectionString, LinqToDB.DataProvider.PostgreSQL.PostgreSQLVersion.v18);

    // Return your PostgresDb context
    return new PostgresDbService(options);
});

var app = builder.Build();

// Print Settings nicely
if (app.Environment.IsDevelopment())
{
    var options = new JsonSerializerOptions { WriteIndented = true };
    string jsonSettings = JsonSerializer.Serialize(appSettings, options);
    
    Console.WriteLine("=== Application Settings Loaded ===");
    Console.WriteLine(jsonSettings);
    Console.WriteLine("===================================");
}

// Create a temporary scope to get the DB service and run the initializer
// Create a scope to access the Scoped IDbService
using (var scope = app.Services.CreateScope())
{
    var dbService = scope.ServiceProvider.GetRequiredService<IDbService>();

    // Only initialize if the "users" table is missing
    if (!DatabaseInitializer.IsDatabaseInitialized(dbService))
    {
        Console.WriteLine("🗄️  Database not found. Running first-time setup...");
        DatabaseInitializer.Initialize(dbService);
    }
    else
    {
        Console.WriteLine("✅ Database already initialized. Skipping setup.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}