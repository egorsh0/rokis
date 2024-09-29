using idcc.Application.Interfaces;
using Microsoft.OpenApi.Models;

namespace idcc.Endpoints;

public static class ReportEndpont
{
    public static void RegisterReportEndpoints(this IEndpointRouteBuilder routes)
    {
        var reports = routes.MapGroup("/api/v1/report");
      
        reports.MapGet("generate", async (int sessionId, IIdccReport idccReport) =>
        {
            var report = await idccReport.GenerateAsync(sessionId);
            return report is null ? Results.BadRequest() : Results.Ok(report);
        }).WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Generate report",
            Description = "Returns report.",
            Tags = new List<OpenApiTag> { new() { Name = "Report" } }
        });
    }
}