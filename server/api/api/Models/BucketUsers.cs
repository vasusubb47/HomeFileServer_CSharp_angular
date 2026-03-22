using LinqToDB.Mapping;

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
    
    [Column(Name = "permission", DbType = "bucket_perm_type"), NotNull]
    public BucketPermission Permission { get; set; }
    
    [Column(Name = "is_active"), NotNull]
    public bool IsActive { get; set; }
    
    [Column(Name = "created_at", SkipOnInsert = true)]
    public DateTimeOffset CreatedAt { get; set; }

    [Column(Name = "updated_at", SkipOnInsert = true), Nullable]
    public DateTimeOffset? UpdatedAt { get; set; } = null;
    
    [Association(ThisKey = nameof(BucketId), OtherKey = nameof(Bucket.BucketId), CanBeNull = false)]
    public Bucket Bucket { get; set; }

    [Association(ThisKey = nameof(UserId), OtherKey = nameof(User.UserId), CanBeNull = false)]
    public User User { get; set; }
}

public enum BucketPermission
{
    [MapValue("admin")]             
    Admin,        // Matches 'admin' in Postgres
    [MapValue("readonly")]          
    ReadOnly,     // Matches 'readonly' in Postgres
    [MapValue("read_and_write")]    
    ReadAndWrite  // Matches 'read_and_write' in Postgres
}
