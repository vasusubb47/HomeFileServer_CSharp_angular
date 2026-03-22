using LinqToDB.Mapping;

namespace api.Models;

internal interface IBucketId
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
    
    [Column(Name = "bucket_name"), NotNull]
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
    
    [Association(ThisKey = nameof(UserId), OtherKey = nameof(User.UserId), CanBeNull = false)]
    public User Owner { get; set; }
}
