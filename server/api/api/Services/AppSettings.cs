namespace api.Services;

public class AppSettings
{
    public bool IsProd { get; set; }
    public DatabaseSettings DbConn { get; set; } = new();
    public EmailSettings Email { get; set; } = new();
    public JwtSettings Jwt { get; set; } = new();
}

public class DatabaseSettings
{
    public PostgresConnection PostgresConn { get; set; } = new();
    public RedisConnection RedisConn { get; set; } = new();
}

internal interface IDatabaseConfig
{
    string ConnectionString { get; set; }
    int RequestTimeOut { get; set; }
}

public class PostgresConnection : IDatabaseConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public int RequestTimeOut { get; set; } = 0;
}

public class RedisConnection : IDatabaseConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public int RequestTimeOut { get; set; } = 0;
}

public class EmailSettings
{
    public bool SendEmail { get; set; }
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string From { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    
    // email for testing
    public string To { get; set; } = string.Empty;
}

public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryInMinutes { get; set; }
}
