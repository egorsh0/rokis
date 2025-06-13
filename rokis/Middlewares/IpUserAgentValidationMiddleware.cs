namespace rokis.Middlewares;

public class IpUserAgentValidationMiddleware
{
    private readonly RequestDelegate _next;

    public IpUserAgentValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var user = context.User;

        if (user.Identity is { IsAuthenticated: true })
        {
            var ipClaim = user.FindFirst("IP")?.Value;
            var uaClaim = user.FindFirst("UserAgent")?.Value;

            var currentIp = context.Connection.RemoteIpAddress?.ToString();
            var currentUa = context.Request.Headers["User-Agent"].ToString();

            if (ipClaim != currentIp || uaClaim != currentUa)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("IP or User-Agent validation failed.");
                return;
            }
        }

        await _next(context);
    }
}