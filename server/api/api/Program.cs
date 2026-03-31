using System.Text;
using api.Services;
using System.Text.Json;
using api.Repositories.UserRepo;
using api.Services.DatabaseService;
using api.Services.EmailService;
using LinqToDB;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Services

// AppSettings
var appSettings = new AppSettings();
builder.Configuration.GetSection("AppSettings").Bind(appSettings);
builder.Services.AddSingleton(appSettings);


// open Telemetry Setup
// to push logs into open telemetry
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
    logging.AddOtlpExporter(options => {
        options.Endpoint = new Uri("http://localhost:4317");
    });
});

// open telemetry service
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        // service name that we see in open telemetry
        .AddService("HobbyProject-API-test1")
        .AddAttributes(new List<KeyValuePair<string, object>>
        {
            new("deployment.environment", "development"),
            new("test.run_id", DateTime.Now.ToString("yyyyMMdd-HHmm"))
        }))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddNpgsql() 
        .AddSource("LinqToDB") 
        .AddOtlpExporter(options => {
            options.Endpoint = new Uri("http://localhost:4317"); 
            options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        }));

// database service
builder.Services.AddScoped<IDbService>(provider => 
{
    var settings = provider.GetRequiredService<AppSettings>();
    var logger = provider.GetRequiredService<ILogger<PostgresDbService>>();
    var connectionString = settings.DbConn.PostgresConn.ConnectionString;

    var options = new DataOptions()
        .UsePostgreSQL(connectionString, LinqToDB.DataProvider.PostgreSQL.PostgreSQLVersion.v15);

    return new PostgresDbService(options, logger);
});

// adding Auth Service
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer();

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<AppSettings>((options, settings) =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = settings.Jwt.Issuer,
            ValidAudience = settings.Jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Jwt.Key))
        };
    });

builder.Services.AddAuthorization();

// register normal created services
builder.Services.AddScoped<IEmailService, GmailService>();
builder.Services.AddScoped<TokenService>();

// register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddHostedService<DatabaseInitializerService>();

var app = builder.Build();

// keep same ordering
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<JwtTraceMiddleware>();

if (app.Environment.IsDevelopment())
{
    var options = new JsonSerializerOptions { WriteIndented = true };
    string jsonSettings = JsonSerializer.Serialize(appSettings, options);
    
    Console.WriteLine("=== Application Settings Loaded ===");
    Console.WriteLine(jsonSettings);
    Console.WriteLine("===================================");
}

// // Create a temporary scope to get the DB service and run the initializer
// // Create a scope to access the Scoped IDbService
// using (var scope = app.Services.CreateScope())
// {
//     var dbService = scope.ServiceProvider.GetRequiredService<IDbService>();
//
//     // Only initialize if the "users" table is missing
//     if (!DatabaseInitializer.IsDatabaseInitialized(dbService))
//     {
//         Console.WriteLine("🗄️  Database not found. Running first-time setup...");
//         DatabaseInitializer.Initialize(dbService);
//     }
//     else
//     {
//         Console.WriteLine("✅ Database already initialized. Skipping setup.");
//     }
// }

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.UseHttpsRedirection();

app.Run();
