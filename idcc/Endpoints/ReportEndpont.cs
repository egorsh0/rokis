using idcc.Application.Interfaces;
using idcc.Infrastructures;
using idcc.Infrastructures.Interfaces;
using idcc.Models;
using idcc.Repository.Interfaces;
using Microsoft.OpenApi.Models;

namespace idcc.Endpoints;

public static class ReportEndpont
{
    public static void RegisterReportEndpoints(this IEndpointRouteBuilder routes)
    {
        var reports = routes.MapGroup("/api/v1/report");
      
        reports.MapGet("generate", async (int? sessionId, Guid tokenId, bool? full, ISessionRepository sessionRepository, IDataRepository dataRepository, IGraphGenerate graphGenerate,  IIdccReport idccReport) =>
        {
            // Проверка на открытую сессию
            Session? session;
            
            if (sessionId.HasValue)
            {
                session = await sessionRepository.GetSessionAsync(sessionId.Value);
            }
            else
            {
                session = await sessionRepository.GetFinishSessionAsync(tokenId);
            }
            if (session is null)
            {
                return Results.BadRequest(ErrorMessages.SESSION_IS_NOT_EXIST);
            }
            
            if (session.EndTime is not null && session.Score > 0 && !full.HasValue)
            {
                return Results.BadRequest(ErrorMessages.SESSION_IS_FINISHED);
            }
            
            var report = await idccReport.GenerateAsync(session);
            if (report is null)
            {
                return Results.BadRequest(ErrorMessages.REPORT_IS_FAILED);
            }

            await sessionRepository.SessionScoreAsync(session.Id, report.FinalScoreDto!.Score);

            if (report.FinalTopicDatas is null)
                return Results.Ok(new
                {
                    report
                });
            var resize = await dataRepository.GetPercentOrDefaultAsync("GraphSize", 25);
            var img = graphGenerate.Generate(report.FinalTopicDatas, (float)resize);
            return Results.Ok(new
            {
                report, img
            });
        }).WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Generate report",
            Description = "Returns report.",
            Tags = new List<OpenApiTag> { new() { Name = "Report" } }
        });
    }
}