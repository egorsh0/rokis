using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using rokis.Dtos.AdminDto;

namespace rokis.Filters;

public class AdminSecretFilter : IAsyncActionFilter
{
    private readonly string _secret;

    public AdminSecretFilter(IOptions<AdminOptions> options)
    {
        _secret = options.Value.Secret;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue("X-Admin-Secret", out var provided) ||
            provided != _secret)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        await next();
    }
}