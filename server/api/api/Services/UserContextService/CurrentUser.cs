using api.Models;

namespace api.Services.UserContextService;

public class CurrentUser : IUserId, IUserProfile, IIsActive, IUserRole
{
    public Guid UserId { get; set; } = Guid.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
}
