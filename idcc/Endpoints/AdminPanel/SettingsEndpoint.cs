using idcc.Infrastructures;
using idcc.Repository.Interfaces;
using Microsoft.OpenApi.Models;

namespace idcc.Endpoints.AdminPanel;

public static class SettingsEndpoint
{
    public static void RegisterSettingsEndpoints(this IEndpointRouteBuilder routes)
    {
        var settings = routes.MapGroup("/api/v2/settings");

        settings.MapGet("/{type}", async (SettingType type, IDataRepository dataRepository) =>
        {
            var setting = await dataRepository.GetSettingsAsync(type);
            return Results.Ok(setting);

        }).WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Get settings",
            Tags = new List<OpenApiTag> { new() { Name = "Admin" } }
        });
    }
}