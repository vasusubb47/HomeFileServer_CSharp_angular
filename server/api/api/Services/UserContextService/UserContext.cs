using System.Security.Claims;
using api.Models;
using Microsoft.IdentityModel.JsonWebTokens;

namespace api.Services.UserContextService;

public interface IUserContext
{
    CurrentUser? User { get; }
    bool IsAuthenticated { get; }
}

public class UserContext(IHttpContextAccessor httpContextAccessor, ILogger<UserContext> logger) : IUserContext
{
    
    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
    
    public CurrentUser? User
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;

            // Check if user exists and is actually authenticated
            if (user == null || !user.Identity?.IsAuthenticated == true)
                return null;

            return new CurrentUser
            {
                UserId = Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty,
                UserName = user.FindFirstValue(ClaimTypes.Name) ?? "Unknown",
                Email = user.FindFirstValue(ClaimTypes.Email) ?? "",
                Role = Enum.Parse<UserRole>(user.FindFirstValue(ClaimTypes.Role)!, true),
                IsActive = user.FindFirstValue("is_active")?.ToLower() == "true"
            };
        }
    }
}
