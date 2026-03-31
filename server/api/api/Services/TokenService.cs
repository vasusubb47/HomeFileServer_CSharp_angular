using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using api.Models;
using Microsoft.IdentityModel.Tokens;

namespace api.Services;

public class TokenService(AppSettings appSettings, ILogger<TokenService> logger)
{
    private readonly AppSettings _appSettings = appSettings;
    private readonly ILogger<TokenService> _logger = logger;

    public string CreateToken(BasicUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.Jwt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // var claims = new[]
        // {
        //     new Claim(JwtRegisteredClaimNames.Sub, username),
        //     new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        //     new Claim(ClaimTypes.Role, role)
        // };
        Guid jwtId = Guid.NewGuid();
        
        _logger.LogInformation("Creating new token for user {userId},{userName} with JWT token ID : {JWT_ID}", user.UserId, user.UserName, jwtId);
        
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, jwtId.ToString()),
            
            // Custom payload data
            new Claim("is_active", user.IsActive.ToString().ToLower()),
            new Claim("user_id", user.UserId.ToString()) // Explicit ID if preferred
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(_appSettings.Jwt.ExpiryInMinutes.ToString())),
            Issuer = _appSettings.Jwt.Issuer,
            Audience = _appSettings.Jwt.Audience,
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
