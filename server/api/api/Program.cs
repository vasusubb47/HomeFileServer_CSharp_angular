using System.Text;
using api.Services;
using System.Text.Json;
using api.Repositories.UserRepo;
using api.Services.DatabaseService;
using api.Services.EmailService;
using api.Services.UserContextService;
using LinqToDB;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Npgsql;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;
using api.Models;
using api.Repositories.BucketRepo;
using api.Repositories.FileRepo;
using api.Services.BackgroundServices.EmailProcessingService;
using api.Services.BackgroundServices.FileProcessingService;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddOpenApi();

// Services

// AppSettings
var appSettings = new AppSettings();
builder.Configuration.GetSection("AppSettings").Bind(appSettings);
builder.Services.AddSingleton(appSettings);

var redisConnection = ConnectionMultiplexer.Connect(appSettings.DbConn.RedisConn.ConnectionString);
builder.Services.AddSingleton<IConnectionMultiplexer>(redisConnection);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.ConnectionMultiplexerFactory = () => Task.FromResult((IConnectionMultiplexer)redisConnection);
    options.InstanceName = appSettings.DbConn.RedisConn.InstanceName;
});


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
        .AddSource("EmailService.Worker")
        .AddSource("FileProcessing.Worker")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddNpgsql()
        .AddRedisInstrumentation(redisConnection)
        .AddSource("LinqToDB") 
        .AddOtlpExporter(options => {
            options.Endpoint = new Uri("http://localhost:4317"); 
            options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        }));

// database service
NpgsqlConnection.GlobalTypeMapper.MapEnum<UserRole>("user_role_type");
NpgsqlConnection.GlobalTypeMapper.MapEnum<BucketPermission>("bucket_perm_type");
builder.Services.AddScoped<IDbService>(provider => 
{
    var settings = provider.GetRequiredService<AppSettings>();
    var logger = provider.GetRequiredService<ILogger<PostgresDbService>>();
    var connectionString = settings.DbConn.PostgresConn.ConnectionString;

    var options = new DataOptions()
        .UsePostgreSQL(connectionString, LinqToDB.DataProvider.PostgreSQL.PostgreSQLVersion.v15);

    return new PostgresDbService(options, logger);
});

builder.Services.AddScoped<RedisCacheService>();

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

// Required to inject IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Register your typed context
builder.Services.AddScoped<IUserContext, UserContext>();

// register normal created services
// builder.Services.AddScoped<IEmailService, GmailService>();
builder.Services.AddScoped<IEmailService, QueuedEmailService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<FileService>();
builder.Services.AddSingleton<FileProcessingChannel>();
builder.Services.AddSingleton<EmailProcessingChannel>();

// register background workers
builder.Services.AddHostedService<FileHashWorker>();
builder.Services.AddHostedService<EmailWorker>();

// register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBucketRepository, BucketRepository>();
builder.Services.AddScoped<IFileRepository, FileRepository>();

builder.Services.AddHostedService<DatabaseInitializerService>();

var app = builder.Build();

// 1. Static files/Routing
app.UseHttpsRedirection();
app.UseRouting(); // Add this if not using Minimal APIs for everything

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.Run();
