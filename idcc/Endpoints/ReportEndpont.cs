using System.ComponentModel.DataAnnotations;
using idcc.Application.Interfaces;
using idcc.Builders;
using idcc.Dtos;
using idcc.Extensions;
using idcc.Infrastructures;
using idcc.Repository.Interfaces;
using idcc.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;

namespace idcc.Endpoints;

/// <summary>
/// Формирование и получение итоговых отчётов.
/// </summary>
[ApiController]
[Authorize]
[Route("api/report")]
[Tags("Report")]
public class ReportController : ControllerBase
{
    private readonly HybridCache _cache;
    private readonly ISessionService _sessionService;
    private readonly IDataRepository _dataRepo;
    private readonly IGraphService _graphs;
    private readonly IConfigRepository _cfgRepo;
    private readonly IReportRepository _reports;
    private readonly IUserAnswerRepository _answers;
    private readonly IMetricService _metrics;
    
    private readonly IReportService  _reportService;
    private readonly IChartService _chartService;

    public ReportController(
        HybridCache cache,
        ISessionService sessionService,
        IDataRepository dataRepository,
        IGraphService graphService,
        IConfigRepository configRepository,
        IReportRepository reportRepository,
        IUserAnswerRepository answerRepository,
        IMetricService metricService,
        IReportService reportService,
        IChartService chartService)
    {
        _cache = cache;
        _sessionService = sessionService;
        _dataRepo = dataRepository;
        _graphs = graphService;
        _cfgRepo = configRepository;
        _reports = reportRepository;
        _answers = answerRepository;
        _metrics = metricService;
        
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
            await _cache.GetOrCreateAsync<(MessageCode? code, ReportGeneratedDto? dto)>(cacheKey,
            async _ =>
            {
                // TODO
                // отчёт уже есть?
                // if (await _reports.ExistsForTokenAsync(tokenId))
                // {
                //     return (MessageCode.REPORT_ALREADY_EXISTS, null);
                // }

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
                    var datas = await _reportService.GetAllTopicDataAsync(session);
                    var normalizers = new TopicNormalizerBuilder().Build(datas);
                    img1 = _chartService.DrawUserProfile(report.FinalTopicDatas, normalizers, report.FinalScoreDto.Grade, report.ThinkingPattern, report.CognitiveStabilityIndex);
                    
                    var resize = await _dataRepo.GetPercentOrDefaultAsync("Reasonable", 2);
                    img2 = _graphs.Generate(report.CognitiveStabilityIndex, report.ThinkingPattern,
                                            report.FinalScoreDto.Grade, report.FinalTopicDatas, resize);
                    img3 = _graphs.GenerateRadarChartForFinalTopics(report.FinalTopicDatas,
                                            report.CognitiveStabilityIndex, report.ThinkingPattern,
                                            report.FinalScoreDto.Grade, resize);
                }

                // 5. пишем в БД
                var grades = await _cfgRepo.GetGradesAsync();
                var gradeId = grades.FirstOrDefault(g => g.Name == report.FinalScoreDto.Grade)?.Id ?? 0;

                await _reports.SaveReportAsync(tokenId, report.FinalScoreDto.Score, gradeId, img1);

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
            return BadRequest(new ResponseDto(errorCode.Value, errorCode.Value.GetDescription()));

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
        var dto = await _cache.GetOrCreateAsync<ReportShortDto?>(cacheKey, async _ =>
        {
            var questions = await _answers.GetQuestionResults(session);
            var csIndex = _metrics.CalculateCognitiveStability(questions);
            var pattern = _metrics.DetectThinkingPattern(questions, csIndex);

            var rr = await _reports.GetByTokenAsync(tokenId);
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