using LinqToDB.Mapping;

namespace api.Models;

internal interface IFileId
{
    Guid FileId { get; set; }
}

public class File: IFileId, IUserId, IBucketId, IIsPublic, IModelTimeInfo
{
    public Guid FileId { get; set; } = Guid.Empty;
    public Guid UserId { get; set; } = Guid.Empty;
    public Guid BucketId { get; set; } = Guid.Empty;
    public bool IsPublic { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; } = null;
}
