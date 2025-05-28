using idcc.Context;
using idcc.Dtos;
using idcc.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace idcc.Repository;

public class ConfigRepository : IConfigRepository
{
    private readonly IdccContext _context;
    private readonly ILogger<ConfigRepository> _log;

    public ConfigRepository(IdccContext context, ILogger<ConfigRepository> log)
    {
        _context = context;
        _log  = log;
    }
    
    public async Task<List<AnswerTimeDto>> GetAnswerTimesAsync()
    {
        var answerTimes = await _context.AnswerTimes.Select(time => new AnswerTimeDto(time.Grade.Name, time.Average, time.Min, time.Max))
            .ToListAsync();

        return [..answerTimes];
    }

    public async Task<List<CountDto>> GetCountsAsync()
    {
        var counts = await _context.Counts.Select(count => new CountDto(count.Code, count.Description, count.Value)).ToListAsync();
        return [..counts];
    }

    public async Task<List<GradeDto>> GetGradesAsync()
    {
        var grades = await _context.Grades.Select(g => new GradeDto(g.Id, g.Name, g.Code, g.Description))
            .ToListAsync();
        return [..grades];
    }

    public async Task<List<GradeLevelDto>> GetGradeLevelsAsync()
    {
        var gradeLevels = await _context.GradeLevels.Select(level => new GradeLevelDto(level.Grade.Name, level.Level))
            .ToListAsync();
        return [..gradeLevels];
    }

    public async Task<List<GradeRelationDto>> GetGradeRelationsAsync()
    {
        var gradeRelations = await _context.GradeRelations.Select(relation => new GradeRelationDto(relation.Start!.Name, relation.End!.Name))
            .ToListAsync();
        return [..gradeRelations];
    }

    public async Task<List<PersentDto>> GetPersentsAsync()
    {
        var persents = await _context.Persents.Select(persent => new PersentDto(persent.Code, persent.Description, persent.Value))
            .ToListAsync();
        return [..persents];
    }

    public async Task<List<DirectionDto>> GetDirectionsAsync()
    {
        var roles = await _context.Directions.Select(role => new DirectionDto(role.Id, role.Name, role.Code, role.Description, role.BasePrice)).ToListAsync();
        return [..roles];
    }

    public async Task<List<WeightDto>> GetWeightsAsync()
    {
        var weights = await _context.Weights.Select(weight => new WeightDto(weight.Grade.Name, weight.Min, weight.Max))
            .ToListAsync();
        return [..weights];
    }
    
    public async Task<List<DiscountRuleDto>> GetDiscountsRuleAsync()
    {
        var discountRules = await _context.DiscountRules.Select(discountRule => new DiscountRuleDto(discountRule.MinQuantity, discountRule.MaxQuantity, discountRule.DiscountRate))
            .ToListAsync();
        return [..discountRules];
    }
    
    public async Task<List<MailingDto>> GetMailingRulesAsync()
    {
        var mailingRules = await _context.MailingSettings.Select(mailingRule => new MailingDto(mailingRule.MailingCode, mailingRule.IsEnabled, mailingRule.Subject, mailingRule.Body))
                .ToListAsync();
        return [..mailingRules];
    }
    
    public async Task<MailingDto?> GetMailingAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        // Поиск шаблона (без учёта регистра) + IsEnabled = true
        var entity = await _context.MailingSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(m =>
                m.MailingCode == code && m.IsEnabled);

        if (entity is not null)
        {
            return new MailingDto(
                entity.MailingCode,
                entity.IsEnabled,
                entity.Subject,
                entity.Body);
        }
        _log.LogWarning("Mailing template {Code} not found or disabled", code);
        return null;
    }
    
    public async Task<List<TopicDto>> GetTopicsAsync()
    {
        var topics = await _context.Topics.Select(topic => new TopicDto(topic.Name, topic.Description, topic.Direction.Id))
            .ToListAsync();
        return [..topics];
    }
}