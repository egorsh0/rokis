using idcc.Infrastructures;
using idcc.Repository.Interfaces;
using Microsoft.OpenApi.Models;

namespace idcc.Endpoints.AdminPanel;

public static class SettingsEndpoint
{
    public static void RegisterSettingsEndpoints(this IEndpointRouteBuilder routes)
    {
        var settings = routes.MapGroup("/api/v2/settings");

        settings.MapGet("/{type}", async (SettingType type, ISettingsRepository settingsRepository) =>
        {
            switch (type)
            {
                case SettingType.AnswerTime:
                    var at = await settingsRepository.GetAnswerTimesAsync();
                    return Results.Ok(at);
                case SettingType.Count:
                    var cts = await settingsRepository.GetCountsAsync();
                    return Results.Ok(cts);
                case SettingType.GradeLevel:
                    var gl = await settingsRepository.GetGradeLevelsAsync();
                    return Results.Ok(gl);
                case SettingType.Persent:
                    var prst = await settingsRepository.GetPersentsAsync();
                    return Results.Ok(prst);
                case SettingType.Weight:
                    var wght = await settingsRepository.GetWeightsAsync();
                    return Results.Ok(wght);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }).WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Get settings",
            Tags = new List<OpenApiTag> { new() { Name = "Admin" } }
        });
    }
}