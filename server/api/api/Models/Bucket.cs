using LinqToDB;
using LinqToDB.Mapping;

namespace api.Models;

public interface IBucketId
{
    Guid BucketId { get; set; }
}

internal interface IBucketInfo
{
    string BucketName { get; set; }
    bool IsShared { get; set; }
}

[Table(Name = "buckets")]
public class Bucket: IBucketId, IUserId, IBucketInfo, IIsPublic, IModelTimeInfo, IIsActive
{
    [PrimaryKey]
    [Column(Name = "bucket_id", SkipOnInsert = true)]
    public Guid BucketId { get; set; } = Guid.Empty;
    
    [Column(Name = "user_id"), NotNull]
    public Guid UserId { get; set; } = Guid.Empty;
    
    [Column(Name = "bucket_name", DataType = DataType.VarChar, Length = 125), NotNull]
    public string BucketName { get; set; } = string.Empty;
    
    [Column(Name = "is_public"), NotNull]
    public bool IsPublic { get; set; }
    
    [Column(Name = "is_shared"), NotNull]
    public bool IsShared { get; set; }
    
    [Column(Name = "is_active"), NotNull]
    public bool IsActive { get; set; }
    
    [Column(Name = "created_at", SkipOnInsert = true)]
    public DateTimeOffset CreatedAt { get; set; }

    [Column(Name = "updated_at", SkipOnInsert = true), Nullable]
    public DateTimeOffset? UpdatedAt { get; set; } = null;
    
    [Association(ThisKey = nameof(UserId), OtherKey = nameof(BasicUser.UserId), CanBeNull = false)]
    public BasicUser Owner { get; set; }
}

public record InsertBucketInfo : IBucketInfo, IIsPublic
{
    public string BucketName { get; set; } = string.Empty;
    public bool IsShared { get; set; }
    public bool IsPublic { get; set; }
}

public record InsertUserBucketInfo : IUserId, IBucketInfo, IIsPublic
{
    public Guid UserId { get; set; } = Guid.Empty;
    public string BucketName { get; set; } = string.Empty;
    public bool IsShared { get; set; }
    public bool IsPublic { get; set; }
    
    internal static InsertUserBucketInfo MapFrom<T>(T user, Guid ownerId)
        where T : IBucketInfo, IIsPublic
    {
        return new InsertUserBucketInfo
        {
            UserId = ownerId,
            BucketName = user.BucketName,
            IsShared = user.IsShared,
            IsPublic = user.IsPublic
        };
    }
}

public record BucketInfo
{
}