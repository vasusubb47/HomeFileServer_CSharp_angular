using api.Models;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Interceptors;
using System.Data.Common;

namespace api.Services.DatabaseService;

public class PostgresDbService: DataConnection, IDbService
{
    public PostgresDbService(DataOptions options) : base(options) 
    {
        // Add your SQL tracer/interceptor
        this.AddInterceptor(new UnwrappedCommandInterceptor());
    }
    
    public ITable<User> Users => this.GetTable<User>();
    public ITable<Bucket> Buckets => this.GetTable<Bucket>();
    public ITable<BucketUsers> BucketUsers => this.GetTable<BucketUsers>();
    public ITable<UserFile> UserFiles => this.GetTable<UserFile>();
    public ITable<FileMetadata> FilesMetadata => this.GetTable<FileMetadata>();
}

// Your Trace Interceptor (Exactly like your SQLite version)
public class UnwrappedCommandInterceptor : CommandInterceptor
{
    public override DbCommand CommandInitialized(CommandEventData eventData, DbCommand command)
    {
        Console.WriteLine("\n--- [POSTGRES SQL] ---");
        Console.WriteLine(command.CommandText);
        Console.WriteLine("----------------------\n");
        return base.CommandInitialized(eventData, command);
    }
}
