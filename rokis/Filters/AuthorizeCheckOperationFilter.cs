using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace rokis.Filters;

public class AuthorizeCheckOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Проверяем, есть ли на уровне метода или контроллера атрибут [Authorize]
        var hasAuthorize = context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<AuthorizeAttribute>()
            .Any();

        // Или атрибут [Authorize] на уровне всего контроллера:
        if (!hasAuthorize)
        {
            hasAuthorize = context.MethodInfo.DeclaringType?
                .GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .Any() ?? false;
        }

        if (!hasAuthorize)
        {
            // Если нет [Authorize] - выходим, не добавляем security requirements
            return;
        }

        // Если [Authorize] есть – добавляем requirements
        operation.Security = new List<OpenApiSecurityRequirement>
        {
            new()
            {
                [new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer" // То же, что в AddSecurityDefinition
                        }
                    }
                ] = new List<string>()
            }
        };
    }
}