using idcc.Application.Interfaces;
using idcc.Infrastructures;
using idcc.Repository.Interfaces;
using Microsoft.OpenApi.Models;

namespace idcc.Endpoints;

public static class ReportEndpont
{
    public static void RegisterReportEndpoints(this IEndpointRouteBuilder routes)
    {
        var reports = routes.MapGroup("/api/v1/report");
      
        reports.MapGet("generate", async (int sessionId, ISessionRepository sessionRepository, IIdccReport idccReport) =>
        {
            // Проверка на открытую сессию
            var session = await sessionRepository.GetSessionAsync(sessionId);
            if (session is null)
            {
                return Results.BadRequest(ErrorMessage.SESSION_IS_NOT_EXIST);
            }
            
            if (session.EndTime is null)
            {
                return Results.BadRequest(ErrorMessage.SESSION_IS_NOT_FINISHED);
            }
            
            var report = await idccReport.GenerateAsync(session);
            return report is null ? Results.BadRequest() : Results.Ok(report);
        }).WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Generate report",
            Description = "Returns report.",
            Tags = new List<OpenApiTag> { new() { Name = "Report" } }
        });
    }
}