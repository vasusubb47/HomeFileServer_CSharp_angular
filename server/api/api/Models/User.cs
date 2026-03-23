using LinqToDB;
using LinqToDB.Mapping;

namespace api.Models;

internal interface IUserId
{
    Guid UserId { get; set; }
}

internal interface IUserProfile
{
    string UserName { get; set; }
    string Email { get; set; }
}

internal interface IUserSecurity
{
    string Passcode { get; set; }
}

[Table(Name = "users")]
public class User : IUserId, IUserProfile, IUserSecurity, IModelTimeInfo, IIsActive
{
    [PrimaryKey]
    [Column(Name = "user_id", SkipOnInsert = true)]
    public Guid UserId { get; set; } = Guid.Empty;
    
    [Column(Name = "user_name", DataType = DataType.VarChar, Length = 125), NotNull]
    public string UserName { get; set; } = string.Empty;
    
    [Column(Name = "email", DataType = DataType.VarChar, Length = 125), NotNull]
    public string Email { get; set; } = string.Empty;
    
    [Column(Name = "passcode", DataType = DataType.VarChar, Length = 255), NotNull]
    public string Passcode { get; set; } = string.Empty;
    
    [Column(Name = "is_active"), NotNull]
    public bool IsActive { get; set; }
    
    [Column(Name = "created_at", SkipOnInsert = true)]
    public DateTimeOffset CreatedAt { get; set; }

    [Column(Name = "updated_at", SkipOnInsert = true), Nullable]
    public DateTimeOffset? UpdatedAt { get; set; } = null;

}
