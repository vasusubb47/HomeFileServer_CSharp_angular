using api.Models;
using api.UtilityClass;
using api.UtilityClass.databaseErrors;

namespace api.Repositories.BucketRepo;

public interface IBucketRepository
{
    public Task<Result<Bucket, IDatabaseError>> InsertBucket(InsertUserBucketInfo info);
    
    public Task<List<Bucket>> GetAllBucketsForUser(Guid userId);
    public Task<bool> IsBucketOwner(Guid userId, Guid bucketId);
}
