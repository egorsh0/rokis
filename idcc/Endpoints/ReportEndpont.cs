using idcc.Application.Interfaces;
using idcc.Dtos;
using idcc.Infrastructures;
using idcc.Infrastructures.Interfaces;
using idcc.Models;
using idcc.Repository.Interfaces;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.OpenApi.Models;

namespace idcc.Endpoints;

public static class ReportEndpont
{
    public static void RegisterReportEndpoints(this IEndpointRouteBuilder routes)
    {
        var reports = routes.MapGroup("/api/v1/report");
      
        reports.MapGet("generate", async (int? sessionId, Guid tokenId, bool? full, ISessionRepository sessionRepository, IDataRepository dataRepository, IGraphGenerate graphGenerate,  IConfigRepository configRepository, IReportRepository reportRepository, IIdccReport idccReport) =>
        {
            // ---------- 1.  Находим сессию ----------
            Session? session = sessionId.HasValue
                ? await sessionRepository.GetSessionAsync(sessionId.Value)
                : await sessionRepository.GetFinishSessionAsync(tokenId);

            if (session is null)
            {
                return Results.BadRequest(ErrorMessages.SESSION_IS_NOT_EXIST);
            }

            if (session.EndTime is not null && session.Score > 0 && !full.HasValue)
            {
                return Results.BadRequest(ErrorMessages.SESSION_IS_FINISHED);
            }
            
            // ---------- 1.1  Проверяем: отчёт уже существует? ----------
            if (await reportRepository.ExistsForTokenAsync(session.TokenId))
                return Results.BadRequest(ErrorMessages.REPORT_ALREADY_EXISTS);
            
            // ---------- 2.  Генерируем отчёт ----------
            var report = await idccReport.GenerateAsync(session);
            if (report is null)
            {
                return Results.BadRequest(ErrorMessages.REPORT_IS_FAILED);
            }

            // ---------- 3.  Обновляем Score в сессии ----------
            await sessionRepository.SessionScoreAsync(session.Id, report.FinalScoreDto!.Score);

            // ---------- 4.  Генерируем график при необходимости ----------
            byte[]? imgBytes = null;
            if (report.FinalTopicDatas is not null)
            {
                var resize = await dataRepository.GetPercentOrDefaultAsync("GraphSize", 25);
                imgBytes   = graphGenerate.Generate(report.FinalTopicDatas, (float)resize);
            }
            
            // ---------- 5.  Сохраняем ReportResult ----------
            var grades = await configRepository.GetGradesAsync();
            var grade = grades.FirstOrDefault(g => g.Name == report.FinalScoreDto.Grade);
            int gradeId = 0;
            if (grade is not null)
            {
                gradeId = grade.Id;
            }
            await reportRepository.SaveReportAsync(
                tokenId : session.TokenId,
                score   : report.FinalScoreDto!.Score,
                gradeId : gradeId,
                image   : imgBytes);
            
            // ---------- 6.  Возврат клиенту ----------
            return Results.Ok(imgBytes is null
                ? new { report }
                : new { report, img = imgBytes });
        }).WithOpenApi(x => new OpenApiOperation(x)
        {
            Summary = "Generate report",
            Description = "Returns report.",
            Tags = new List<OpenApiTag> { new() { Name = "Report" } }
        });
        
        reports.MapGet("get", async (
                int?                 sessionId,
                Guid?                tokenId,
                HybridCache       cache,
                IReportRepository    reportRepository) =>
        {
            if (sessionId is null && tokenId is null)
            {
                return Results.BadRequest("Specify sessionId or tokenId");
            }

            // ── формируем ключ ─────────────────────────────────────
            //  * если передан tokenId → "Report:Token:<guid>"
            //  * если передан sessionId → "Report:Session:<id>"
            var cacheKey = tokenId is not null
                ? $"Report:Token:{tokenId}"
                : $"Report:Session:{sessionId}";
            // ── пробуем достать из кэша или создать ─────────────────
            var reportDto = await cache.GetOrCreateAsync<ReportShortDto?>(cacheKey,
                async _ =>
                {
                    var rr = tokenId is not null
                        ? await reportRepository.GetByTokenAsync(tokenId.Value)
                        : await reportRepository.GetBySessionAsync(sessionId!.Value);

                    if (rr is null)
                    {
                        return null;
                    }

                    return new ReportShortDto(
                        rr.TokenId,
                        rr.Score,
                        rr.Grade.Name,
                        rr.Image is null ? null : Convert.ToBase64String(rr.Image));
                });

            return reportDto is null ? Results.BadRequest(ErrorMessages.REPORT_NOT_FOUND) : Results.Ok(reportDto);
        })
            .WithOpenApi(x => new OpenApiOperation(x)
            {
                Summary     = "Get saved report",
                Description = "Возвращает ранее сгенерированный отчёт по tokenId или sessionId",
                Tags = new List<OpenApiTag> { new() { Name = "Report" } }
            });
    }
}