using idcc.Context;
using idcc.Dtos;
using idcc.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace idcc.Repository;

public class ConfigRepository : IConfigRepository
{
    private readonly IdccContext _context;

    public ConfigRepository(IdccContext context)
    {
        _context = context;
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
        var grades = await _context.Grades.Select(g => new GradeDto(g.Name, g.Code, g.Description))
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
        var roles = await _context.Directions.Select(role => new DirectionDto(role.Name, role.Code, role.Description)).ToListAsync();
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
}