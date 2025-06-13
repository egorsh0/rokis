using idcc.Dtos;
using idcc.Repository;
using Microsoft.Extensions.Caching.Hybrid;

namespace idcc.Service;

public interface IConfigService
{
    Task<(double average, double min, double max)?> GetGradeTimeInfoAsync(int gradeId);
    
    Task<(double min, double max)?> GetGradeWeightInfoAsync(int gradeId);
    Task<(double, GradeDto?)> GetGradeLevelAsync(double score);

    Task<double> GetPercentOrDefaultAsync(string code, double value);
    Task<int> GetCountOrDefaultAsync(string code, int value);

    Task<(GradeDto? prev, GradeDto? next)> GetRelationAsync(GradeDto current);
    
    Task<List<AnswerTimeDto>> GetAnswerTimesAsync();
    Task<List<CountDto>> GetCountsAsync();
    Task<List<GradeDto>> GetGradesAsync();
    Task<List<GradeLevelDto>> GetGradeLevelsAsync();
    Task<List<GradeRelationDto>> GetGradeRelationsAsync();
    Task<List<PersentDto>> GetPersentsAsync();
    Task<List<DirectionDto>> GetDirectionsAsync();
    Task<List<WeightDto>> GetWeightsAsync();
    Task<WeightDto?> GetWeightsAsync(int gradeId);
    Task<List<DiscountRuleDto>> GetDiscountRulesAsync();
    Task<List<MailingDto>> GetMailingRulesAsync();
    Task<MailingDto?> GetMailingAsync(string code);
    Task<List<TopicDto>> GetTopicsAsync();
    Task<TopicDto?> GetTopicAsync(int topicId);
    Task<List<TimeDto>> GetTimesAsync();
    Task<TimeDto?> GetTimesAsync(string code);
}

public class ConfigService : IConfigService
{
    private readonly IConfigRepository _configRepository;
    private readonly HybridCache _hybridCache;
    private readonly ILogger<ConfigService> _logger;

    public ConfigService(IConfigRepository configRepository, 
        HybridCache hybridCache,
        ILogger<ConfigService> logger)
    {
        _configRepository = configRepository;
        _hybridCache = hybridCache;
        _logger = logger;
    }

    public async Task<(double average, double min, double max)?> GetGradeTimeInfoAsync(int gradeId)
    {
        _logger.LogInformation("GetGradeTimeInfoAsync");
        var cachekey = $"{nameof(ConfigService)}.{nameof(GetGradeTimeInfoAsync)}.{gradeId}";
        return await _hybridCache.GetOrCreateAsync<(double average, double min, double max)?>(cachekey, async _ =>
        {
            var answerTimes = await GetAnswerTimesAsync();
            var answerTime = answerTimes.FirstOrDefault(time => time.Grade.Id == gradeId);
            if (answerTime is null)
            {
                return null;
            }

            return (answerTime.Average, answerTime.Min, answerTime.Max);
        });
    }

    public async Task<(double min, double max)?> GetGradeWeightInfoAsync(int gradeId)
    {
        _logger.LogInformation("GetGradeWeightInfoAsync");
        var cachekey = $"{nameof(ConfigService)}.{nameof(GetGradeWeightInfoAsync)}.{gradeId}";
        return await _hybridCache.GetOrCreateAsync<(double min, double max)?>(cachekey, async _ =>
        {
            var weights = await GetWeightsAsync();
            var weight = weights.FirstOrDefault(w => w.Grade.Id == gradeId);
            if (weight is null)
            {
                return null;
            }
            return (weight.Min, weight.Max);
            
        });
    }

    public async Task<(double, GradeDto?)> GetGradeLevelAsync(double score)
    {
        _logger.LogInformation("GetGradeLevelAsync");
        var cachekey = $"{nameof(ConfigService)}.{nameof(GetGradeLevelAsync)}.{score}";
        return await _hybridCache.GetOrCreateAsync<(double, GradeDto?)>(cachekey, async _ =>
        {
            var gradeLevels = await GetGradeLevelsAsync();
            var gradeLevel = gradeLevels.FirstOrDefault(gl => score >= gl.Min && score < gl.Max);

            if (gradeLevel != null)
            {
                return (score, gradeLevel.Grade);
            }
            _logger.LogWarning($"Не удалось определить грейд для score = {score}");
            return (0, null);
        });
    }

    public async Task<double> GetPercentOrDefaultAsync(string code, double value)
    {
        _logger.LogInformation("GetPercentOrDefaultAsync");
        var cachekey = $"{nameof(ConfigService)}.{nameof(GetPercentOrDefaultAsync)}.{code}";
        return await _hybridCache.GetOrCreateAsync(cachekey, async _ =>
        {
            var persents = await GetPersentsAsync();
            var persent = persents.FirstOrDefault(p => p.Code == code);
            return persent?.Value ?? value;
        });
    }

    public async Task<int> GetCountOrDefaultAsync(string code, int value)
    {
        _logger.LogInformation("GetCountOrDefaultAsync");
        var cachekey = $"{nameof(ConfigService)}.{nameof(GetCountOrDefaultAsync)}.{code}";
        return await _hybridCache.GetOrCreateAsync(cachekey, async _ =>
        {
            var counts = await GetCountsAsync();
            var count = counts.FirstOrDefault(c => c.Code == code);
            return count?.Value ?? value;
        });
    }

    public async Task<(GradeDto? prev, GradeDto? next)> GetRelationAsync(GradeDto current)
    {
        _logger.LogInformation("GetRelationAsync");
        var cachekey = $"{nameof(ConfigService)}.{nameof(GetRelationAsync)}.{current.Id}";
        return await _hybridCache.GetOrCreateAsync<(GradeDto? prev, GradeDto? next)>(cachekey, async _ =>
        {
            var gradeRelations = await _configRepository.GetGradeRelationsAsync();
            var next = gradeRelations
                .Where(r => r.Start != null && r.Start.Id == current.Id)
                .Select(x => x.End)
                .FirstOrDefault();

            var prev = gradeRelations
                .Where(r => r.End != null && r.End.Id == current.Id)
                .Select(x => x.Start)
                .FirstOrDefault();

            GradeDto? nextDto = null;
            GradeDto? prevDto = null;

            if (next != null)
            {
                nextDto = new GradeDto(next.Id, next.Name, next.Description, next.Description);
            }

            if (prev != null)
            {
                prevDto = new GradeDto(prev.Id, prev.Name, prev.Description, prev.Description);
            }

            return (prevDto, nextDto);
        });
    }

    public async Task<List<AnswerTimeDto>> GetAnswerTimesAsync()
    {
        _logger.LogInformation("GetAnswerTimesAsync");
        var cachekey = $"{nameof(ConfigService)}.{nameof(GetAnswerTimesAsync)}";
        return await _hybridCache.GetOrCreateAsync<List<AnswerTimeDto>>(cachekey, async _ =>
        {
            var answerTimes = await _configRepository.GetAnswerTimesAsync();
            return answerTimes.Select(time => new AnswerTimeDto(new GradeDto(time.Grade.Id, time.Grade.Name, time.Grade.Code, time.Grade.Description), time.Average, time.Min, time.Max)).ToList();
        });
    }

    public async Task<List<CountDto>> GetCountsAsync()
    {
        _logger.LogInformation("GetCountsAsync");
        var cachekey = $"{nameof(ConfigService)}.{nameof(GetCountsAsync)}";
        return await _hybridCache.GetOrCreateAsync<List<CountDto>>(cachekey, async _ =>
        {
            var counts = await _configRepository.GetCountsAsync();
            return counts.Select(count => new CountDto(count.Code, count.Description, count.Value)).ToList();
        });
    }

    public async Task<List<GradeDto>> GetGradesAsync()
    {
        _logger.LogInformation("GetGradesAsync");
        var cachekey = $"{nameof(ConfigService)}.{nameof(GetGradesAsync)}";
        return await _hybridCache.GetOrCreateAsync<List<GradeDto>>(cachekey, async _ =>
        {
            var grades = await _configRepository.GetGradesAsync();
            return grades.Select(g => new GradeDto(g.Id, g.Name, g.Code, g.Description)).ToList();
        });
    }

    public async Task<List<GradeLevelDto>> GetGradeLevelsAsync()
    {
        _logger.LogInformation("GetGradeLevelsAsync");
        var cachekey = $"{nameof(ConfigService)}.{nameof(GetGradeLevelsAsync)}";
        return await _hybridCache.GetOrCreateAsync<List<GradeLevelDto>>(cachekey, async _ =>
        {
            var gradeLevels = await _configRepository.GetGradeLevelsAsync();
            return gradeLevels.Select(level => new GradeLevelDto(new GradeDto(level.Grade.Id,level.Grade.Name, level.Grade.Code, level.Grade.Description), level.Min, level.Max)).ToList();
        });
    }

    public async Task<List<GradeRelationDto>> GetGradeRelationsAsync()
    {
        _logger.LogInformation("GetGradeRelationsAsync");
        var cachekey = $"{nameof(ConfigService)}.{nameof(GetGradeRelationsAsync)}";
        return await _hybridCache.GetOrCreateAsync<List<GradeRelationDto>>(cachekey, async _ =>
        {
            var gradeRelations = await _configRepository.GetGradeRelationsAsync();
            return gradeRelations.Select(relation => new GradeRelationDto(relation.Start?.Name, relation.End?.Name)).ToList();
        });
    }

    public async Task<List<PersentDto>> GetPersentsAsync()
    {
        _logger.LogInformation("GetPersentsAsync");
        var cachekey = $"{nameof(ConfigService)}.{nameof(GetPersentsAsync)}";
        return await _hybridCache.GetOrCreateAsync<List<PersentDto>>(cachekey, async _ =>
        {
            var persents = await _configRepository.GetPersentsAsync();
            return persents.Select(persent => new PersentDto(persent.Code, persent.Description, persent.Value)).ToList();
        });
    }

    public async Task<List<DirectionDto>> GetDirectionsAsync()
    {
        _logger.LogInformation("GetDirectionsAsync");
        var cachekey = $"{nameof(ConfigService)}.{nameof(GetDirectionsAsync)}";
        return await _hybridCache.GetOrCreateAsync<List<DirectionDto>>(cachekey, async _ =>
        {
            var directions = await _configRepository.GetDirectionsAsync();
            return directions.Select(role => new DirectionDto(role.Id, role.Name, role.Code, role.Description, role.BasePrice)).ToList();
        });
    }

    public async Task<List<WeightDto>> GetWeightsAsync()
    {
        _logger.LogInformation("GetWeightsAsync");
        var cachekey = $"{nameof(ConfigService)}.{nameof(GetWeightsAsync)}";
        return await _hybridCache.GetOrCreateAsync<List<WeightDto>>(cachekey, async _ =>
        {
            var weights = await _configRepository.GetWeightsAsync();
            return weights.Select(weight => new WeightDto(
                weight.Grade.Id,
                new GradeDto(weight.Grade.Id, weight.Grade.Name, weight.Grade.Code, weight.Grade.Description), 
                weight.Min, 
                weight.Max)).ToList();
        });
    }
    
    public async Task<WeightDto?> GetWeightsAsync(int gradeId)
    {
        var weights = await GetWeightsAsync();
        var weight = weights.FirstOrDefault(rule => rule.Id == gradeId);
        if (weight != null)
        {
            return weight;
        }
        _logger.LogWarning("Weight by {gradeId} not found or disabled", gradeId);
        return null;
    }

    public async Task<List<DiscountRuleDto>> GetDiscountRulesAsync()
    {
        _logger.LogInformation("GetDiscountsRuleAsync");
        var cachekey = $"{nameof(ConfigService)}.{nameof(GetDiscountRulesAsync)}";
        return await _hybridCache.GetOrCreateAsync<List<DiscountRuleDto>>(cachekey, async _ =>
        {
            var discounts = await _configRepository.GetDiscountsRuleAsync();
            return discounts.Select(discountRule => new DiscountRuleDto(discountRule.MinQuantity, discountRule.MaxQuantity, discountRule.DiscountRate)).ToList();
        });
    }

    public async Task<List<MailingDto>> GetMailingRulesAsync()
    {
        _logger.LogInformation("GetMailingRulesAsync");
        var cachekey = $"{nameof(ConfigService)}.{nameof(GetMailingRulesAsync)}";
        return await _hybridCache.GetOrCreateAsync<List<MailingDto>>(cachekey, async _ =>
        {
            var mailingRules = await _configRepository.GetMailingRulesAsync();
            return mailingRules.Select(mailingRule => new MailingDto(mailingRule.MailingCode, mailingRule.IsEnabled, mailingRule.Subject, mailingRule.Body)).ToList();
        });
    }

    public async Task<MailingDto?> GetMailingAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }
        
        var mailingRules = await GetMailingRulesAsync();
        var mailingRule = mailingRules.FirstOrDefault(rule => rule.MailingCode == code && rule.IsEnabled);
        if (mailingRule != null)
        {
            return mailingRule;
        }
        _logger.LogWarning("Mailing template {Code} not found or disabled", code);
        return null;
    }

    public async Task<List<TopicDto>> GetTopicsAsync()
    {
        _logger.LogInformation("GetTopicsAsync");
        var cachekey = $"{nameof(ConfigService)}.{nameof(GetTopicsAsync)}";
        return await _hybridCache.GetOrCreateAsync<List<TopicDto>>(cachekey, async _ =>
        {
            var topics = await _configRepository.GetTopicsAsync();
            return topics.Select(topic => new TopicDto(topic.Id, topic.Name, topic.Description, topic.Direction.Id)).ToList();
        });
    }

    public async Task<TopicDto?> GetTopicAsync(int topicId)
    {
        var topics = await GetTopicsAsync();
        var topic = topics.FirstOrDefault(t => t.Id == topicId);
        if (topic != null)
        {
            return topic;
        }
        _logger.LogWarning("Topic by {topic} not found or disabled", topic);
        return null;
    }

    public async Task<List<TimeDto>> GetTimesAsync()
    {
        _logger.LogInformation("GetTimesAsync");
        var cachekey = $"{nameof(ConfigService)}.{nameof(GetTimesAsync)}";
        return await _hybridCache.GetOrCreateAsync<List<TimeDto>>(cachekey, async _ =>
        {
            var times = await _configRepository.GetTimesAsync();
            return times.Select(time => new TimeDto(time.Code, time.Description, time.Value)).ToList();
        });
    }

    public async Task<TimeDto?> GetTimesAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }
        var times = await GetTimesAsync();
        var time = times.FirstOrDefault(t => t.Code == code);
        if (time != null)
        {
            return time;
        }
        _logger.LogWarning("Time template {Code} not found or disabled", code);
        return null;
    }
}