using LinqToDB;
using LinqToDB.Mapping;

namespace api.Models;

internal interface IUserId
{
    Guid UserId { get; set; }
}

internal interface IUserProfile : IUserEmail
{
    string UserName { get; set; }
}

internal interface IUserEmail
{
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

public record InsertUser : IUserProfile, IUserSecurity
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Passcode { get; set; } = string.Empty;
}

public record UserLogin : IUserEmail, IUserSecurity
{
    public string Email { get; set; } = string.Empty;
    public string Passcode { get; set; } = string.Empty;
}

public record BasicUser : IUserId, IUserProfile, IIsActive
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    internal static BasicUser MapFrom<T>(T user)
        where T : IUserId, IUserProfile, IIsActive
    {
        return new BasicUser
        {
            UserId = user.UserId,
            UserName = user.UserName,
            Email = user.Email,
            IsActive = user.IsActive,
        };
    }
}

public record SelectUser : IUserId, IUserProfile, IIsActive, IModelTimeInfo
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    
    internal static SelectUser MapFrom<T> (T user)
        where T : IUserId, IUserProfile, IIsActive, IModelTimeInfo
    {
        return new SelectUser
        {
            UserId = user.UserId,
            UserName = user.UserName,
            Email = user.Email,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
        };
    }
}
