using idcc.Application.Interfaces;
using idcc.Infrastructures;
using idcc.Infrastructures.Interfaces;
using idcc.Models;
using idcc.Models.Dto;
using idcc.Repository.Interfaces;
using Microsoft.OpenApi.Models;

namespace idcc.Endpoints;

public static class ReportEndpont
{
    public static void RegisterReportEndpoints(this IEndpointRouteBuilder routes)
    {
        var reports = routes.MapGroup("/api/v1/report");
      
        reports.MapGet("generate", async (int? sessionId, string username, ISessionRepository sessionRepository, IGraphGenerate graphGenerate,  IIdccReport idccReport) =>
        {
            // Проверка на открытую сессию
            Session? session= null;
            
            if (sessionId.HasValue)
            {
                session = await sessionRepository.GetSessionAsync(sessionId.Value);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(username))
                {
                    session = await sessionRepository.GetSessionAsync(username);
                }
            }
            if (session is null)
            {
                return Results.BadRequest(new ErrorMessage()
                {
                    Message = ErrorMessages.SESSION_IS_NOT_EXIST
                });
            }
            
            if (session.EndTime is not null && session.Score > 0)
            {
                return Results.BadRequest(new ErrorMessage()
                {
                    Message = ErrorMessages.SESSION_IS_FINISHED
                });
            }
            
            var report = await idccReport.GenerateAsync(session);
            if (report is null)
            {
                return Results.BadRequest(new ErrorMessage()
                {
                    Message = ErrorMessages.REPORT_IS_FAILED
                });
            }

            await sessionRepository.SessionScoreAsync(sessionId!.Value, report.FinalScoreDto!.Score);

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