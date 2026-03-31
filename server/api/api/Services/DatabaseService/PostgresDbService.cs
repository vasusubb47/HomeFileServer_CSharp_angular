using api.Models;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Interceptors;
using System.Data.Common;

namespace api.Services.DatabaseService;

public class PostgresDbService: DataConnection, IDbService
{
    private readonly ILogger<PostgresDbService> _logger;
    
    public PostgresDbService(DataOptions options, ILogger<PostgresDbService> logger) : base(options) 
    {
        // Add your SQL tracer/interceptor
        _logger = logger;
        this.AddInterceptor(new UnwrappedCommandInterceptor(_logger));
    }
    
    public ITable<User> Users => this.GetTable<User>();
    public ITable<Bucket> Buckets => this.GetTable<Bucket>();
    public ITable<BucketUsers> BucketUsers => this.GetTable<BucketUsers>();
    public ITable<UserFile> UserFiles => this.GetTable<UserFile>();
    public ITable<FileMetadata> FilesMetadata => this.GetTable<FileMetadata>();
}

// Your Trace Interceptor (Exactly like your SQLite version)
public class UnwrappedCommandInterceptor(ILogger<PostgresDbService> logger) : CommandInterceptor
{
    private readonly ILogger<PostgresDbService> _logger = logger;

    public override DbCommand CommandInitialized(CommandEventData eventData, DbCommand command)
    {
        
        _logger.LogDebug("postgres SQL : {command}", command.CommandText);
        
        // Console.WriteLine("\n--- [POSTGRES SQL] ---");
        // Console.WriteLine(command.CommandText);
        // Console.WriteLine("----------------------\n");
        return base.CommandInitialized(eventData, command);
    }
}
