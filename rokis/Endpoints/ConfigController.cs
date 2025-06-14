using rokis.Dtos;
using rokis.Service;
using Microsoft.AspNetCore.Mvc;

namespace rokis.Endpoints;

/// <summary>Читает справочники / конфигурационные данные, которые редко меняются.</summary>
/// <remarks>Каждый метод использует HybridCache (In-Memory + Redis). TTL указан в конфигурации.</remarks>
[ApiController]
[Route("config")]
public class ConfigController : ControllerBase
{
    private IConfigService _configService;

    public ConfigController(IConfigService configService)
    {
        _configService = configService;
    }

    /*────────── 1. AnswerTimes ──────────*/
    /// <summary>Время ответа на вопросы (по диапазонам).</summary>
    /// <remarks>Используется движком для расчёта K-коэффициента.</remarks>
    [HttpGet("answerTimes")]
    [ProducesResponseType(typeof(IEnumerable<AnswerTimeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<List<AnswerTimeDto>> GetAnswerTimesAsync()
    {
        return await _configService.GetAnswerTimesAsync();
    }

    /*────────── 2. Counts ──────────*/
    /// <summary>Справочник счётчиков.</summary>
    [HttpGet("counts")]
    [ProducesResponseType(typeof(IEnumerable<CountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<List<CountDto>> GetCountsAsync()
    {
        return await _configService.GetCountsAsync();
    }

    /*────────── 3. Grades ──────────*/
    /// <summary>Список грейдов (Junior/Middle/Senior…).</summary>
    [HttpGet("grades")]
    [ProducesResponseType(typeof(IEnumerable<GradeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<List<GradeDto>> GetGradesAsync()
    {
        return await _configService.GetGradesAsync();
    }

    /*────────── 4. GradeLevels ──────────*/
    /// <summary>Уровни внутри грейда.</summary>
    [HttpGet("gradeLevels")]
    [ProducesResponseType(typeof(IEnumerable<GradeLevelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<List<GradeLevelDto>> GetGradeLevelsAsync()
    {
        return await _configService.GetGradeLevelsAsync();
    }

    /*────────── 5. GradeRelations ──────────*/
    /// <summary>Связка «Grade → GradeLevel → Score диапазон».</summary>
    [HttpGet("gradeRelations")]
    [ProducesResponseType(typeof(IEnumerable<GradeRelationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<List<GradeRelationDto>> GetGradeRelationsAsync()
    {
        return await _configService.GetGradeRelationsAsync();
    }

    /*────────── 6. Percents ──────────*/
    /// <summary>Процентные коэффициенты.</summary>
    [HttpGet("percents")]
    [ProducesResponseType(typeof(IEnumerable<PersentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<List<PersentDto>> GetPersentsAsync()
    {
        return await _configService.GetPersentsAsync();
    }

    /*────────── 7. Directions ──────────*/
    /// <summary>Направления (QA, DEV, SA…).</summary>
    [HttpGet("direction")]
    [ProducesResponseType(typeof(IEnumerable<DirectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<List<DirectionDto>> GetDirectionsAsync()
    {
        return await _configService.GetDirectionsAsync();
    }

    /*────────── 8. Weights ──────────*/
    /// <summary>Весовые коэффициенты.</summary>
    [HttpGet("weights")]
    [ProducesResponseType(typeof(IEnumerable<WeightDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<List<WeightDto>> GetWeightsAsync()
    {
        return await _configService.GetWeightsAsync();
    }

    /*────────── 9. Discount rules ──────────*/
    /// <summary>Шкала скидок при покупке токенов.</summary>
    [HttpGet("discountsRule")]
    [ProducesResponseType(typeof(IEnumerable<DiscountRuleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<List<DiscountRuleDto>> GetDiscountsRulesAsync()
    {
        return await _configService.GetDiscountRulesAsync();
    }
    
    /*────────── 10. Mailing rules ──────────*/
    /// <summary>Правила email-рассылок.</summary>
    [HttpGet("mails")]
    [ProducesResponseType(typeof(IEnumerable<MailingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<List<MailingDto>> GetMailingRulesAsync()
    {
        return await _configService.GetMailingRulesAsync();
    }
    
    /*────────── 11. Topics ──────────*/
    /// <summary>Список тем тестирования.</summary>
    [HttpGet("topics")]
    [ProducesResponseType(typeof(IEnumerable<TopicDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<List<TopicDto>> GetTopicsAsync()
    {
        return await _configService.GetTopicsAsync();
    }
    
    /*────────── 12. Times ──────────*/
    /// <summary>Временные настройки системы.</summary>
    [HttpGet("times")]
    [ProducesResponseType(typeof(IEnumerable<TimeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<List<TimeDto>> GetTimesAsync()
    {
        return await _configService.GetTimesAsync();
    }
}