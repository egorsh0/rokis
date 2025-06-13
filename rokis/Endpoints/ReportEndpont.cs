using System.ComponentModel.DataAnnotations;
using rokis.Application.Interfaces;
using rokis.Dtos;
using rokis.Extensions;
using rokis.Infrastructures;
using rokis.Repository;
using rokis.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;

namespace rokis.Endpoints;

/// <summary>
/// Формирование и получение итоговых отчётов.
/// </summary>
[ApiController]
[Authorize]
[Route("api/report")]
[Tags("Report")]
public class ReportController : ControllerBase
{
    private readonly HybridCache _hybridCache;
    private readonly ISessionService _sessionService;
    private readonly IConfigRepository _configRepository;
    private readonly IReportRepository _reportRepository;
    private readonly IUserAnswerRepository _userAnswerRepository;
    private readonly IMetricService _metricService;
    
    private readonly IReportService  _reportService;
    private readonly IChartService _chartService;

    public ReportController(
        HybridCache hybridCache,
        ISessionService sessionService,
        IConfigRepository configRepository,
        IReportRepository reportRepositoryRepository,
        IUserAnswerRepository userAnswerRepositoryRepository,
        IMetricService metricServiceService,
        IReportService reportService,
        IChartService chartService)
    {
        _hybridCache = hybridCache;
        _sessionService = sessionService;
        _configRepository = configRepository;
        _reportRepository = reportRepositoryRepository;
        _userAnswerRepository = userAnswerRepositoryRepository;
        _metricService = metricServiceService;
        
        _reportService = reportService;
        _chartService = chartService;
    }

    // ════════════════════════════════════════════════════════════════
    //  GET /api/report/generate
    // ════════════════════════════════════════════════════════════════
    /// <summary>Генерирует (или пытается сгенерировать) PDF-отчёт.</summary>
    /// <remarks>
    /// <para>
    /// • Принимает <paramref name="tokenId"/>.<br/>
    /// • Если отчёт уже существует — <c>400</c>.<br/>
    /// • Сохраняет полный отчёт в БД, но не кеширует в HybridCache
    ///   (кеш держит только DTO).
    /// </para>
    /// </remarks>
    [HttpGet("generate")]
    [ProducesResponseType(typeof(ReportGeneratedDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto),       StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Generate([FromQuery, Required] Guid tokenId)
    {
        // 1. находим финальную сессию
        SessionDto? session = await _sessionService.GetFinishSessionAsync(tokenId);

        if (session is null)
        {
            return BadRequest(new ResponseDto(MessageCode.SESSION_IS_NOT_EXIST,
                                              MessageCode.SESSION_IS_NOT_EXIST.GetDescription()));
        }

        if (session.EndTime is null)
        {
            return BadRequest(new ResponseDto(MessageCode.SESSION_HAS_ACTIVE,
                                              MessageCode.SESSION_HAS_ACTIVE.GetDescription()));
        }

        var cacheKey = $"Report:Full:Token:{tokenId}";
        var (errorCode, reportDto) =
            await _hybridCache.GetOrCreateAsync<(MessageCode? code, ReportGeneratedDto? dto)>(cacheKey,
            async _ =>
            {
                // 2. генерируем отчёт
                var report = await _reportService.GenerateAsync(session);
                if (report is null)
                {
                    return (MessageCode.REPORT_IS_FAILED, null);
                }

                // 3. обновляем Score в сессии
                await _sessionService.SessionScoreAsync(session.Id, report.FinalScoreDto!.Score);

                // 4. графики (если нужны)
                byte[]? img1 = null, img2 = null, img3 = null;
                if (report.FinalTopicDatas is not null)
                {
                    img1 = _chartService.DrawUserProfile(report.FinalTopicDatas, report.FinalScoreDto.Grade, report.ThinkingPattern, report.CognitiveStabilityIndex);
                }
                
                // // 5. пишем в БД, если нету
                if (!await _reportRepository.ExistsForTokenAsync(tokenId))
                {
                    var grades = await _configRepository.GetGradesAsync();
                    var gradeId = grades.FirstOrDefault(g => g.Name == report.FinalScoreDto.Grade)?.Id ?? 0;
                    await _reportRepository.SaveReportAsync(tokenId, report.FinalScoreDto.Score, gradeId, img1);
                }
                
                // 6. возвращаем DTO
                var dto = new ReportGeneratedDto(
                    report,
                    img1 is null ? null : Convert.ToBase64String(img1),
                    img3 is null ? null : Convert.ToBase64String(img3),
                    img2 is null ? null : Convert.ToBase64String(img2));

                // dto кэшируем, code=null
                return (null, dto);
            });

        if (errorCode.HasValue)
        {
            return BadRequest(new ResponseDto(errorCode.Value, errorCode.Value.GetDescription()));
        }

        return Ok(reportDto);
    }

    // ════════════════════════════════════════════════════════════════
    //  GET /api/report/get
    // ════════════════════════════════════════════════════════════════
    /// <summary>Получить ранее сформированный отчёт.</summary>
    [HttpGet("get")]
    [ProducesResponseType(typeof(ReportShortDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseDto),     StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get([FromQuery, Required] Guid tokenId)
    {
        // 1. проверяем, что сессия завершена
        var session = await _sessionService.GetSessionAsync(tokenId);
        if (session is null)
        {
            return BadRequest(new ResponseDto(MessageCode.SESSION_IS_NOT_EXIST,
                                              MessageCode.SESSION_IS_NOT_EXIST.GetDescription()));
        }

        if (session.EndTime is null)
        {
            return BadRequest(new ResponseDto(MessageCode.SESSION_HAS_ACTIVE,
                                              MessageCode.SESSION_HAS_ACTIVE.GetDescription()));
        }

        var cacheKey = $"Report:Short:Token:{tokenId}";
        var dto = await _hybridCache.GetOrCreateAsync<ReportShortDto?>(cacheKey, async _ =>
        {
            var questions = await _userAnswerRepository.GetQuestionResults(session);
            var csIndex = _metricService.CalculateCognitiveStability(questions);
            var pattern = _metricService.DetectThinkingPattern(questions, csIndex);

            var rr = await _reportRepository.GetByTokenAsync(tokenId);
            return rr is null ? null : new ReportShortDto(
                rr.TokenId, rr.Score, rr.Grade.Name,
                csIndex, pattern,
                rr.Image is null ? null : Convert.ToBase64String(rr.Image));
        });

        return dto is null
            ? BadRequest(new ResponseDto(MessageCode.REPORT_NOT_FOUND,
                                         MessageCode.REPORT_NOT_FOUND.GetDescription()))
            : Ok(dto);
    }
}