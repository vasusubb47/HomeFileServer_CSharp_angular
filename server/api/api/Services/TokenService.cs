using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using api.Models;
using Microsoft.IdentityModel.Tokens;

namespace api.Services;

public class TokenService(AppSettings appSettings, ILogger<TokenService> logger)
{

    public string CreateToken(BasicUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.Jwt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        Guid jwtId = Guid.NewGuid();
        
        logger.LogDebug("Creating new token for user {userId},{userName} with JWT token ID : {JWT_ID}", user.UserId, user.UserName, jwtId);
        
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, jwtId.ToString()),
            
            // Custom payload data
            new Claim("is_active", user.IsActive.ToString().ToLower()),
            new Claim("role", user.Role.ToString().ToLower()),
            new Claim("user_id", user.UserId.ToString()) // Explicit ID if preferred
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(appSettings.Jwt.ExpiryInMinutes.ToString())),
            Issuer = appSettings.Jwt.Issuer,
            Audience = appSettings.Jwt.Audience,
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
