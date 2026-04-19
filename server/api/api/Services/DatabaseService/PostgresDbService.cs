using api.Models;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Interceptors;
using System.Data.Common;
using LinqToDB.Internal.SqlQuery;

namespace api.Services.DatabaseService;

public class PostgresDbService: DataConnection, IDbService
{
    
    public PostgresDbService(DataOptions options, ILogger<PostgresDbService> logger) : base(options) 
    {
        // Add your SQL tracer/interceptor
        this.AddInterceptor(new UnwrappedCommandInterceptor(logger));
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

    public override DbCommand CommandInitialized(CommandEventData eventData, DbCommand command)
    {
        logger.LogDebug("postgres SQL : {command}", command.CommandText);
        return base.CommandInitialized(eventData, command);
    }
}
