using LinqToDB.Mapping;

namespace api.Models;

public interface IFileId
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

    [Association(ThisKey = nameof(UserId), OtherKey = nameof(BasicUser.UserId), CanBeNull = false)]
    public BasicUser Owner { get; set; }
}

public record InsertUserFile: IFileId, IFileMetadata
{
    public Guid FileId { get; set; } = Guid.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileType { get; set; } = string.Empty;
    public List<string>? FileHashes { get; set; } = [];
}

public record UserBucketId : IUserId, IBucketId
{
    public Guid UserId { get; set; } = Guid.Empty;
    public Guid BucketId { get; set; } = Guid.Empty;
}

public record UserBucketFileId : IUserId, IBucketId, IFileId
{
    public Guid UserId { get; set; } = Guid.Empty;
    public Guid BucketId { get; set; } = Guid.Empty;
    public Guid FileId { get; set; } = Guid.Empty;
}

public record UserFileNameExt : IUserId, IBucketId, IFileId
{
    public Guid UserId { get; set; } = Guid.Empty;
    public Guid BucketId { get; set; } = Guid.Empty;
    public Guid FileId { get; set; } = Guid.Empty;
    public string FileExtenstion { get; set; } = string.Empty;
    
    // Add this to store the trace context
    public string? TraceId { get; set; }
    public string? SpanId { get; set; }
}
