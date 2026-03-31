using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;

public class JwtTraceMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Check if the user is authenticated
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // 2. Extract the JTI claim
            var jti = context.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (!string.IsNullOrEmpty(jti))
            {
                // 3. Attach it to the current OTel Span (Activity)
                Activity.Current?.SetTag("auth.jti", jti);
                
                // Optional: You can also add the User ID while you're at it
                var userId = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    Activity.Current?.SetTag("user.id", userId);
                }
            }
        }

        await next(context);
    }
}
