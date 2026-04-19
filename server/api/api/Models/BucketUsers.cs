using LinqToDB;
using LinqToDB.Mapping;
using NpgsqlTypes;

namespace api.Models;

internal interface IBucketUserPermission
{
    public BucketPermission Permission { get; set; }
}

[Table(Name = "bucket_users")]
public class BucketUsers : IBucketId, IUserId, IBucketUserPermission, IModelTimeInfo, IIsActive
{
    [PrimaryKey(Order = 1)]
    [Column(Name = "bucket_id"), NotNull]
    public Guid BucketId { get; set; } = Guid.Empty;
    
    [PrimaryKey(Order = 2)]
    [Column(Name = "user_id"), NotNull]
    public Guid UserId { get; set; } = Guid.Empty;
    
    // [Column(Name = "permission", DataType = DataType.Enum , DbType = "bucket_perm_type"), NotNull]
    [Column(Name = "permission", DataType = DataType.Enum), NotNull]
    public BucketPermission Permission { get; set; }
    
    [Column(Name = "is_active"), NotNull]
    public bool IsActive { get; set; }
    
    [Column(Name = "created_at", SkipOnInsert = true)]
    public DateTimeOffset CreatedAt { get; set; }

    [Column(Name = "updated_at", SkipOnInsert = true), Nullable]
    public DateTimeOffset? UpdatedAt { get; set; } = null;
    
    [LinqToDB.Mapping.Association(ThisKey = nameof(BucketId), OtherKey = nameof(Bucket.BucketId), CanBeNull = false)]
    public Bucket Bucket { get; set; }

    [LinqToDB.Mapping.Association(ThisKey = nameof(UserId), OtherKey = nameof(User.UserId), CanBeNull = false)]
    public User User { get; set; }
}

public enum BucketPermission
{
    [PgName("admin")]
    Admin,
    [PgName("read_only")]
    ReadOnly,
    [PgName("read_and_write")]
    ReadAndWrite
}
