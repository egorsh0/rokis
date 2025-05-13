using idcc.Dtos;
using idcc.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;

namespace idcc.Endpoints;

/// <summary>Читает справочники / конфигурационные данные, которые редко меняются.</summary>
/// <remarks>Каждый метод использует HybridCache (In-Memory + Redis). TTL указан в конфигурации.</remarks>
[ApiController]
[Route("api/config")]
public class ConfigController : ControllerBase
{
    private readonly IConfigRepository _configRepository;
    private readonly HybridCache _hybridCache;
    private readonly ILogger<ConfigController> _logger;

    public ConfigController(IConfigRepository configRepository, 
        HybridCache hybridCache,
        ILogger<ConfigController> logger)
    {
        _configRepository = configRepository;
        _hybridCache = hybridCache;
        _logger = logger;
    }

    /*────────── 1. AnswerTimes ──────────*/
    /// <summary>Время ответа на вопросы (по диапазонам).</summary>
    /// <remarks>Используется движком для расчёта K-коэффициента.</remarks>
    [HttpGet("answerTimes")]
    [ProducesResponseType(typeof(IEnumerable<AnswerTimeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<List<AnswerTimeDto>> GetAnswerTimesAsync()
    {
        _logger.LogInformation("GetAnswerTimesAsync");
        var cachekey = $"{nameof(ConfigController)}.{nameof(GetAnswerTimesAsync)}";
        return await _hybridCache.GetOrCreateAsync<List<AnswerTimeDto>>(cachekey, async _ =>
            await _configRepository.GetAnswerTimesAsync());
    }

    /*────────── 2. Counts ──────────*/
    /// <summary>Справочник счётчиков.</summary>
    [HttpGet("counts")]
    [ProducesResponseType(typeof(IEnumerable<CountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<List<CountDto>> GetCountsAsync()
    {
        _logger.LogInformation("GetCountsAsync");
        var cachekey = $"{nameof(ConfigController)}.{nameof(GetCountsAsync)}";
        return await _hybridCache.GetOrCreateAsync<List<CountDto>>(cachekey, async _ =>
            await _configRepository.GetCountsAsync());
    }

    /*────────── 3. Grades ──────────*/
    /// <summary>Список грейдов (Junior/Middle/Senior…).</summary>
    [HttpGet("grades")]
    [ProducesResponseType(typeof(IEnumerable<GradeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<List<GradeDto>> GetGradesAsync()
    {
        _logger.LogInformation("GetGradesAsync");
        var cachekey = $"{nameof(ConfigController)}.{nameof(GetGradesAsync)}";
        return await _hybridCache.GetOrCreateAsync<List<GradeDto>>(cachekey, async _ =>
            await _configRepository.GetGradesAsync());
    }

    /*────────── 4. GradeLevels ──────────*/
    /// <summary>Уровни внутри грейда.</summary>
    [HttpGet("gradeLevels")]
    [ProducesResponseType(typeof(IEnumerable<GradeLevelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<List<GradeLevelDto>> GetGradeLevelsAsync()
    {
        _logger.LogInformation("GetGradeLevelsAsync");
        var cachekey = $"{nameof(ConfigController)}.{nameof(GetGradeLevelsAsync)}";
        return await _hybridCache.GetOrCreateAsync<List<GradeLevelDto>>(cachekey, async _ =>
            await _configRepository.GetGradeLevelsAsync());
    }

    /*────────── 5. GradeRelations ──────────*/
    /// <summary>Связка «Grade → GradeLevel → Score диапазон».</summary>
    [HttpGet("gradeRelations")]
    [ProducesResponseType(typeof(IEnumerable<GradeRelationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<List<GradeRelationDto>> GetGradeRelationsAsync()
    {
        _logger.LogInformation("GetGradeRelationsAsync");
        var cachekey = $"{nameof(ConfigController)}.{nameof(GetGradeRelationsAsync)}";
        return await _hybridCache.GetOrCreateAsync<List<GradeRelationDto>>(cachekey, async _ =>
            await _configRepository.GetGradeRelationsAsync());
    }

    /*────────── 6. Percents ──────────*/
    /// <summary>Процентные коэффициенты.</summary>
    [HttpGet("percents")]
    [ProducesResponseType(typeof(IEnumerable<PersentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<List<PersentDto>> GetPersentsAsync()
    {
        _logger.LogInformation("GetPersentsAsync");
        var cachekey = $"{nameof(ConfigController)}.{nameof(GetPersentsAsync)}";
        return await _hybridCache.GetOrCreateAsync<List<PersentDto>>(cachekey, async _ =>
            await _configRepository.GetPersentsAsync());
    }

    /*────────── 7. Directions ──────────*/
    /// <summary>Направления (QA, DEV, SA…).</summary>
    [HttpGet("direction")]
    [ProducesResponseType(typeof(IEnumerable<DirectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<List<DirectionDto>> GetDirectionsAsync()
    {
        _logger.LogInformation("GetDirectionsAsync");
        var cachekey = $"{nameof(ConfigController)}.{nameof(GetDirectionsAsync)}";
        return await _hybridCache.GetOrCreateAsync<List<DirectionDto>>(cachekey, async _ =>
            await _configRepository.GetDirectionsAsync());
    }

    /*────────── 8. Weights ──────────*/
    /// <summary>Весовые коэффициенты.</summary>
    [HttpGet("weights")]
    [ProducesResponseType(typeof(IEnumerable<WeightDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<List<WeightDto>> GetWeightsAsync()
    {
        _logger.LogInformation("GetWeightsAsync");
        var cachekey = $"{nameof(ConfigController)}.{nameof(GetWeightsAsync)}";
        return await _hybridCache.GetOrCreateAsync<List<WeightDto>>(cachekey, async _ =>
            await _configRepository.GetWeightsAsync());
    }

    /*────────── 9. Discount rules ──────────*/
    /// <summary>Шкала скидок при покупке токенов.</summary>
    [HttpGet("discountsRule")]
    [ProducesResponseType(typeof(IEnumerable<DiscountRuleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<List<DiscountRuleDto>> GetDiscountsRulesAsync()
    {
        _logger.LogInformation("GetDiscountsRuleAsync");
        var cachekey = $"{nameof(ConfigController)}.{nameof(GetDiscountsRulesAsync)}";
        return await _hybridCache.GetOrCreateAsync<List<DiscountRuleDto>>(cachekey, async _ =>
            await _configRepository.GetDiscountsRuleAsync());
    }
    
    /*────────── 10. Mailing rules ──────────*/
    /// <summary>Правила email-рассылок.</summary>
    [HttpGet("mails")]
    [ProducesResponseType(typeof(IEnumerable<MailingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<List<MailingDto>> GetMailingRulesAsync()
    {
        _logger.LogInformation("GetMailingRulesAsync");
        var cachekey = $"{nameof(ConfigController)}.{nameof(GetMailingRulesAsync)}";
        return await _hybridCache.GetOrCreateAsync<List<MailingDto>>(cachekey, async _ =>
            await _configRepository.GetMailingRulesAsync());
    }
}