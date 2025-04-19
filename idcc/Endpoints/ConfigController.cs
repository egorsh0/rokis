using idcc.Dtos;
using idcc.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;

namespace idcc.Endpoints;

[ApiController]
[Route("api/v1/config")]
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

    [HttpGet("answerTimes")]
    public async Task<List<AnswerTimeDto>> GetAnswerTimesAsync()
    {
        _logger.LogInformation("GetAnswerTimesAsync");
        var cachekey = $"{nameof(ConfigController)}.{nameof(GetAnswerTimesAsync)}";
        return await _hybridCache.GetOrCreateAsync<List<AnswerTimeDto>>(cachekey, async _ =>
            await _configRepository.GetAnswerTimesAsync());
    }

    [HttpGet("counts")]
    public async Task<List<CountDto>> GetCountsAsync()
    {
        _logger.LogInformation("GetCountsAsync");
        var cachekey = $"{nameof(ConfigController)}.{nameof(GetCountsAsync)}";
        return await _hybridCache.GetOrCreateAsync<List<CountDto>>(cachekey, async _ =>
            await _configRepository.GetCountsAsync());
    }

    [HttpGet("grades")]
    public async Task<List<GradeDto>> GetGradesAsync()
    {
        _logger.LogInformation("GetGradesAsync");
        var cachekey = $"{nameof(ConfigController)}.{nameof(GetGradesAsync)}";
        return await _hybridCache.GetOrCreateAsync<List<GradeDto>>(cachekey, async _ =>
            await _configRepository.GetGradesAsync());
    }

    [HttpGet("gradeLevels")]
    public async Task<List<GradeLevelDto>> GetGradeLevelsAsync()
    {
        _logger.LogInformation("GetGradeLevelsAsync");
        var cachekey = $"{nameof(ConfigController)}.{nameof(GetGradeLevelsAsync)}";
        return await _hybridCache.GetOrCreateAsync<List<GradeLevelDto>>(cachekey, async _ =>
            await _configRepository.GetGradeLevelsAsync());
    }

    [HttpGet("gradeRelations")]
    public async Task<List<GradeRelationDto>> GetGradeRelationsAsync()
    {
        _logger.LogInformation("GetGradeRelationsAsync");
        var cachekey = $"{nameof(ConfigController)}.{nameof(GetGradeRelationsAsync)}";
        return await _hybridCache.GetOrCreateAsync<List<GradeRelationDto>>(cachekey, async _ =>
            await _configRepository.GetGradeRelationsAsync());
    }

    [HttpGet("persents")]
    public async Task<List<PersentDto>> GetPersentsAsync()
    {
        _logger.LogInformation("GetPersentsAsync");
        var cachekey = $"{nameof(ConfigController)}.{nameof(GetPersentsAsync)}";
        return await _hybridCache.GetOrCreateAsync<List<PersentDto>>(cachekey, async _ =>
            await _configRepository.GetPersentsAsync());
    }

    [HttpGet("roles")]
    public async Task<List<RoleDto>> GetRolesAsync()
    {
        _logger.LogInformation("GetRolesAsync");
        var cachekey = $"{nameof(ConfigController)}.{nameof(GetRolesAsync)}";
        return await _hybridCache.GetOrCreateAsync<List<RoleDto>>(cachekey, async _ =>
            await _configRepository.GetRolesAsync());
    }

    [HttpGet("weights")]
    public async Task<List<WeightDto>> GetWeightsAsync()
    {
        _logger.LogInformation("GetWeightsAsync");
        var cachekey = $"{nameof(ConfigController)}.{nameof(GetWeightsAsync)}";
        return await _hybridCache.GetOrCreateAsync<List<WeightDto>>(cachekey, async _ =>
            await _configRepository.GetWeightsAsync());
    }
}