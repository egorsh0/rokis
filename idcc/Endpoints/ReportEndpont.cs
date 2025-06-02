using System.ComponentModel.DataAnnotations;
using idcc.Application.Interfaces;
using idcc.Dtos;
using idcc.Infrastructures;
using idcc.Infrastructures.Interfaces;
using idcc.Models;
using idcc.Repository.Interfaces;
using Microsoft.Extensions.Caching.Hybrid;

namespace idcc.Endpoints;

public static class ReportEndpont
{
    public static void RegisterReportEndpoints(this IEndpointRouteBuilder routes)
    {
        var reports = routes.MapGroup("/api/report")
            .WithTags("Report");
      
        reports.MapGet("generate", 
            /// <summary>Генерация (или попытка) итогового PDF-отчёта.</summary>
            /// <remarks>
            /// <para>
            /// • Принимает <c>sessionId</c> **или** <c>tokenId</c>.<br/>
            /// • Если отчёт уже существует → <c>400 Отчет уже существует.</c>.<br/>
            /// • По завершении сохраняет результат в БД и кеш не затрагивает.
            /// </para>
            /// </remarks>
            /// <param name="tokenId">GUID токена (обязателен, если нет <c>sessionId</c>).</param>
            /// <param name="full">Принудительно пересоздать отчёт, даже если сессия завершена.</param>
            /// <response code="200">Объект отчёта (и base64-картинка, если есть).</response>
            /// <response code="400">Логическая ошибка.</response>  
            async ([Required]Guid tokenId, bool? full, ISessionRepository sessionRepository, IDataRepository dataRepository, IGraphGenerate graphGenerate,  IConfigRepository configRepository, IReportRepository reportRepository, IIdccReport idccReport) =>
        {
            // ---------- 1.  Находим сессию ----------
            Session? session = await sessionRepository.GetFinishSessionAsync(tokenId);

            if (session is null)
            {
                return Results.BadRequest(new ResponseDto(ErrorMessages.SESSION_IS_NOT_EXIST));
            }

            if (session.EndTime is not null && session.Score > 0 && !full.HasValue)
            {
                return Results.BadRequest(new ResponseDto(ErrorMessages.SESSION_IS_FINISHED));
            }
            
            // ---------- 1.1  Проверяем: отчёт уже существует? ----------
            if (await reportRepository.ExistsForTokenAsync(session.TokenId))
                return Results.BadRequest(new ResponseDto(ErrorMessages.REPORT_ALREADY_EXISTS));
            
            // ---------- 2.  Генерируем отчёт ----------
            var report = await idccReport.GenerateAsync(session);
            if (report is null)
            {
                return Results.BadRequest(new ResponseDto(ErrorMessages.REPORT_IS_FAILED));
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
            var dto = new ReportGeneratedDto(report, imgBytes is null
                ? null
                : Convert.ToBase64String(imgBytes));

            return Results.Ok(dto);
        }).Produces<ReportGeneratedDto>()
            .Produces<string>(400);
        
        // ═══════════════════════════════════════════════════════
        //              GET /api/report/get
        // ═══════════════════════════════════════════════════════
        reports.MapGet("get",
                /// <summary>Получить ранее сформированный отчёт из кеша/БД.</summary>
                /// <response code="200">Короткая форма отчёта.</response>
                /// <response code="400">Отчёт не найден / не указаны параметры.</response>
                async (
                [Required]Guid tokenId,
                HybridCache cache,
                IReportRepository reportRepository) =>
        {
            // ── формируем ключ ─────────────────────────────────────
            var cacheKey = $"Report:Token:{tokenId}";
            // ── пробуем достать из кэша или создать ─────────────────
            var dto = await cache.GetOrCreateAsync<ReportShortDto?>(cacheKey, async _ =>
            {
                var rr = await reportRepository.GetByTokenAsync(tokenId);

                return rr is null ? null : new ReportShortDto(
                    rr.TokenId,
                    rr.Score,
                    rr.Grade.Name,
                    rr.Image is null ? null : Convert.ToBase64String(rr.Image));
            });

            return dto is null ? Results.BadRequest(new ResponseDto(ErrorMessages.REPORT_NOT_FOUND)) : Results.Ok(dto);
        }).Produces<ReportShortDto>()
            .Produces<string>(400);
    }
}