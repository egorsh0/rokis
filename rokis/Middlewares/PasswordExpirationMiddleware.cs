using System.Security.Claims;
using rokis.Models;
using Microsoft.AspNetCore.Identity;

namespace rokis.Middlewares;

public class PasswordExpirationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly TimeSpan _expirationPeriod = TimeSpan.FromMinutes(15);
    
    public PasswordExpirationMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task Invoke(HttpContext context, UserManager<ApplicationUser> userManager)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId != null)
            {
                var user = await userManager.FindByIdAsync(userId);
                
                if (user != null)
                {
                    var delta = DateTimeOffset.UtcNow - user.PasswordLastChanged;
                    if (delta > _expirationPeriod)
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsync(new string("Password expired. Please reset your password."));
                        return;
                    }
                }
            }
        }

        await _next(context);
    }
}