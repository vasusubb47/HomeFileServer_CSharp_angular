using api.Models;
using api.Services.DatabaseService;
using api.UtilityClass;
using api.UtilityClass.databaseErrors;
using LinqToDB.Async;
using LinqToDB;
using Npgsql;

namespace api.Repositories.BucketRepo;

public class BucketRepository(IDbService db, ILogger<BucketRepository> logger, RedisCacheService redis) : IBucketRepository
{
    private readonly IDbService _db = db;
    private readonly ILogger<BucketRepository> _logger = logger;
    private readonly RedisCacheService _redis = redis;
    
    public async Task<Result<Bucket, IDatabaseError>> InsertBucket(InsertUserBucketInfo bucketInfo)
    {
        try
        {
            // 1. Perform the insert
            var bucketId = await _db.Buckets
                .Value(b => b.UserId, bucketInfo.UserId)
                .Value(b => b.BucketName, bucketInfo.BucketName)
                .Value(b => b.IsPublic, bucketInfo.IsPublic)
                .Value(b => b.IsShared, bucketInfo.IsShared)
                .Value(b => b.IsActive, true)
                .InsertWithOutputAsync(inserted => inserted.BucketId); // Returns the new ID

            // 2. Fetch the newly created bucket with the Owner association
            var bucket = await _db.Buckets
                .LoadWith(b => b.Owner) // This loads the User info
                .FirstOrDefaultAsync(b => b.BucketId.Equals(bucketId));

            return Result<Bucket, IDatabaseError>.Success(bucket!);
        }
        catch (Exception ex) // Catch base Exception to be safe
        {
            // Search the entire exception tree for a PostgresException
            if (ex.GetBaseException() is PostgresException pgEx || 
                (ex.InnerException is PostgresException innerPgEx && (pgEx = innerPgEx) != null))
            {
                _logger.LogError(ex, "Postgres error occurred during insert");
                return pgEx.SqlState switch
                {
                    PostgresCodes.UniqueViolation => 
                        Result<Bucket, IDatabaseError>.Failure(new UniqueViolationError($"Bucket Name already exists: {bucketInfo.BucketName}")),
        
                    _ => Result<Bucket, IDatabaseError>.Failure(new GeneralDatabaseError(pgEx.MessageText, pgEx.SqlState))
                };
            }

            // If it's not a Postgres error, log and return general failure
            _logger.LogError(ex, "Non-Postgres error occurred during user insertion");
            return Result<Bucket, IDatabaseError>.Failure(new GeneralDatabaseError("An unexpected error occurred."));
        }
    }

    public async Task<List<Bucket>> GetAllBucketsForUser(Guid userId)
    {
        var userBuckets = await _db.Buckets
            .LoadWith(b => b.Owner)
            .Select(bucket => new Bucket
            {
                BucketId = bucket.BucketId,
                UserId = bucket.UserId,
                BucketName = bucket.BucketName,
                IsPublic = bucket.IsPublic,
                IsShared = bucket.IsShared,
                IsActive = bucket.IsActive,
                CreatedAt = bucket.CreatedAt,
                UpdatedAt = bucket.UpdatedAt,
                Owner = bucket.Owner
            })
            .Where(bucket => bucket.UserId == userId)
            .OrderBy(bucket => bucket.CreatedAt)
            .ToListAsync();
        return userBuckets;
    }

    public async Task<bool> IsBucketOwner( Guid bucketId, Guid userId)
    {
        var isOwner = await _db.Buckets
            .Where(b => b.BucketId == bucketId && b.UserId == userId)
            .AnyAsync();
        return isOwner;
    }
}
