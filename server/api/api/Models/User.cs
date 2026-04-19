using LinqToDB;
using LinqToDB.Mapping;
using NpgsqlTypes;

namespace api.Models;

public interface IUserId
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

internal interface IUserRole
{
    UserRole Role { get; set; }
}

[Table(Name = "users")]
public class User : IUserId, IUserProfile, IUserSecurity, IModelTimeInfo, IUserRole, IIsActive
{
    [PrimaryKey]
    [Column(Name = "user_id", SkipOnInsert = true)]
    public Guid UserId { get; set; } = Guid.Empty;
    
    [Column(Name = "user_name", DataType = DataType.VarChar, Length = 125), NotNull]
    public string UserName { get; set; } = string.Empty;
    
    // [Column(Name = "user_role", DataType = DataType.Enum , DbType = "user_role_type"), NotNull]
    [Column(Name = "user_role", DataType = DataType.Enum), NotNull]
    public UserRole Role { get; set; }
    
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

public enum UserRole
{
    [PgName("admin")]
    Admin,
    [PgName("user")]
    User,
}

public record InsertUser : IUserProfile, IUserSecurity, IUserRole
{
    public string UserName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Passcode { get; set; } = string.Empty;
}

public record UserLogin : IUserEmail, IUserSecurity
{
    public string Email { get; set; } = string.Empty;
    public string Passcode { get; set; } = string.Empty;
}

[Table(Name = "users")]
public record BasicUser : IUserId, IUserProfile, IIsActive, IUserRole
{
    [PrimaryKey]
    [Column(Name = "user_id", SkipOnInsert = true)]
    public Guid UserId { get; set; }
    [Column(Name = "user_name", DataType = DataType.VarChar, Length = 125), NotNull]
    public string UserName { get; set; } = string.Empty;
    [Column(Name = "email", DataType = DataType.VarChar, Length = 125), NotNull]
    public string Email { get; set; } = string.Empty;
    [Column(Name = "user_role", DataType = DataType.Enum), NotNull]
    public UserRole Role { get; set; }
    [Column(Name = "is_active"), NotNull]
    public bool IsActive { get; set; }

    internal static BasicUser MapFrom<T>(T user)
        where T : IUserId, IUserProfile, IIsActive, IUserRole
    {
        return new BasicUser
        {
            UserId = user.UserId,
            UserName = user.UserName,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
        };
    }
}

public record SelectUser : IUserId, IUserProfile, IUserRole, IIsActive, IModelTimeInfo
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    
    internal static SelectUser MapFrom<T> (T user)
        where T : IUserId, IUserProfile, IUserRole, IIsActive, IModelTimeInfo
    {
        return new SelectUser
        {
            UserId = user.UserId,
            UserName = user.UserName,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
        };
    }
}
