namespace api.Services;

public class AppSettings
{
    public DatabaseSettings DbConn { get; set; } = new();
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
