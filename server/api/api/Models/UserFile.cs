using LinqToDB.Mapping;

namespace api.Models;

internal interface IFileId
{
    Guid FileId { get; set; }
}

[Table(Name = "user_files")]
public class UserFile: IFileId, IUserId, IBucketId, IIsPublic, IModelTimeInfo
{
    [PrimaryKey]
    [Column(Name = "file_id"), NotNull]
    public Guid FileId { get; set; } = Guid.Empty;
    
    [Column(Name = "user_id"), NotNull]
    public Guid UserId { get; set; } = Guid.Empty;
    
    [Column(Name = "bucket_id"), NotNull]
    public Guid BucketId { get; set; } = Guid.Empty;

    [Column(Name = "is_public"), NotNull] 
    public bool IsPublic { get; set; }

    [Column(Name = "created_at", SkipOnInsert = true)]
    public DateTimeOffset CreatedAt { get; set; }

    [Column(Name = "updated_at", SkipOnInsert = true), Nullable]
    public DateTimeOffset? UpdatedAt { get; set; } = null;
    
    [Association(ThisKey = nameof(BucketId), OtherKey = nameof(Bucket.BucketId), CanBeNull = false)]
    public Bucket Bucket { get; set; }

    [Association(ThisKey = nameof(UserId), OtherKey = nameof(Owner.UserId), CanBeNull = false)]
    public User Owner { get; set; }
}
