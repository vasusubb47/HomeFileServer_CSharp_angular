using api.Models;
using LinqToDB;

namespace api.Services.DatabaseService;

public interface IDbService: IDataContext
{
    ITable<User> Users { get; }
    ITable<Bucket> Buckets { get; }
    ITable<BucketUsers> BucketUsers { get; }
    ITable<UserFile> UserFiles { get; }
    ITable<FileMetadata> FilesMetadata { get; }
}
