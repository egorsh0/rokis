using rokis.Context;
using rokis.Models;
using rokis.Models.Config;
using rokis.Models.Mail;
using Microsoft.EntityFrameworkCore;

namespace rokis.Repository;


public interface IConfigRepository
{
    // Получение конфигурационной информации
    
    Task<List<AnswerTime>> GetAnswerTimesAsync();
    Task<List<Count>> GetCountsAsync();
    Task<List<Grade>> GetGradesAsync();
    Task<List<GradeLevel>> GetGradeLevelsAsync();
    Task<List<GradeRelation>> GetGradeRelationsAsync();
    Task<List<Persent>> GetPercentsAsync();
    Task<List<Direction>> GetDirectionsAsync();
    Task<List<Weight>> GetWeightsAsync();
    Task<List<DiscountRule>> GetDiscountsRuleAsync();
    Task<List<MailingSetting>> GetMailingRulesAsync();
    Task<List<Topic>> GetTopicsAsync();
    Task<List<Time>> GetTimesAsync();
}

public class ConfigRepository : IConfigRepository
{
    private readonly RokisContext _context;

    public ConfigRepository(RokisContext context)
    {
        _context = context;
    }
    
    public async Task<List<AnswerTime>> GetAnswerTimesAsync()
    {
        return await _context.AnswerTimes
            .Select(t => t)
            .Include(gl => gl.Grade)
            .ToListAsync();
    }

    public async Task<List<Count>> GetCountsAsync()
    {
        return await _context.Counts
            .Select(c => c)
            .ToListAsync();
    }

    public async Task<List<Grade>> GetGradesAsync()
    {
        return await _context.Grades
            .Select(g => g)
            .ToListAsync();
    }

    public async Task<List<GradeLevel>> GetGradeLevelsAsync()
    {
        return await _context.GradeLevels
            .Select(level => level)
            .Include(gl => gl.Grade)
            .ToListAsync();
    }

    public async Task<List<GradeRelation>> GetGradeRelationsAsync()
    {
        return await _context.GradeRelations
            .Select(relation => relation)
            .Include(gl => gl.Start)
            .Include(gl => gl.End)
            .ToListAsync();
    }

    public async Task<List<Persent>> GetPercentsAsync()
    {
        return await _context.Persents
            .Select(percent => percent)
            .ToListAsync();
    }

    public async Task<List<Direction>> GetDirectionsAsync()
    {
        return await _context.Directions
            .Select(direction => direction)
            .ToListAsync();
    }

    public async Task<List<Weight>> GetWeightsAsync()
    {
        return await _context.Weights
            .Select(weight => weight)
            .Include(gl => gl.Grade)
            .ToListAsync();
    }
    
    public async Task<List<DiscountRule>> GetDiscountsRuleAsync()
    {
        return await _context.DiscountRules
            .Select(discountRule => discountRule)
            .ToListAsync();
    }
    
    public async Task<List<MailingSetting>> GetMailingRulesAsync()
    {
        return await _context.MailingSettings
            .Select(mailingRule => mailingRule)
            .ToListAsync();
    }
    
    public async Task<List<Topic>> GetTopicsAsync()
    {
        return await _context.Topics
            .Select(topic => topic)
            .Include(gl => gl.Direction)
            .ToListAsync();
    }

    public async Task<List<Time>> GetTimesAsync()
    {
        return await _context.Times
            .Select(time => time)
            .ToListAsync();
    }
}