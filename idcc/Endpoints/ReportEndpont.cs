using idcc.Application.Interfaces;
using idcc.Infrastructures;
using idcc.Infrastructures.Interfaces;
using idcc.Repository.Interfaces;
using Microsoft.OpenApi.Models;

namespace idcc.Endpoints;

public static class ReportEndpont
{
    public static void RegisterReportEndpoints(this IEndpointRouteBuilder routes)
    {
        var reports = routes.MapGroup("/api/v1/report");
      
        reports.MapGet("generate", async (int sessionId, ISessionRepository sessionRepository, IGraphGenerate graphGenerate,  IIdccReport idccReport) =>
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
            if (report is null)
            {
                return Results.BadRequest();
            }

            if (report.FinalTopicDatas is not null)
            {
                var img = graphGenerate.Generate(report.FinalTopicDatas);
                report.TopicReport = img;
            }
            return Results.Ok(report);
        }).WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Generate report",
            Description = "Returns report.",
            Tags = new List<OpenApiTag> { new() { Name = "Report" } }
        });
    }
}