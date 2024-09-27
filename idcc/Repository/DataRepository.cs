using idcc.Context;
using idcc.Models;
using idcc.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace idcc.Repository;

public class DataRepository : IDataRepository
{
    private readonly IdccContext _context;

    public DataRepository(IdccContext context)
    {
        _context = context;
    }
    
    public async Task<(double average, double min, double max)?> GetGradeTimeInfoAsync(int gradeId)
    {
        var answerTime = await _context.AnswerTimes.Where(_ => _.Grade.Id == gradeId).FirstOrDefaultAsync();
        if (answerTime is null)
        {
            return null;
        }

        return (answerTime.Average, answerTime.Min, answerTime.Max);
    }

    public async Task<(double min, double max)?> GetGradeWeightInfoAsync(int gradeId)
    {
        var weight = await _context.Weights.Where(_ => _.Grade.Id == gradeId).FirstOrDefaultAsync();
        if (weight is null)
        {
            return null;
        }

        return (weight.Min, weight.Max);
    }
    
    public async Task<(double, Grade)> GetGradeLevelAsync(double score)
    {
        var gradeLevel = await _context.GradeLevels.Select(_ => _.Level).ToListAsync();
        var value = gradeLevel.OrderBy(x => x)
            .ThenBy(x => Math.Abs(x - score))
            .ElementAt(0);
        
        var grade = await _context.GradeLevels.Where(_ => _.Level.Equals(value)).FirstAsync();
        return (value, grade.Grade);
    }

    public async Task<double> GetPercentOrDefaultAsync(string code, double value)
    {
        var percent = await _context.Settings.FirstOrDefaultAsync(_ => _.Code == code && _.Category == "Percent");
        if (percent is null)
        {
            return value;
        }
        return (double)percent.Value;
    }
    
    public async Task<double> GetCountOrDefaultAsync(string code, double value)
    {
        var count = await _context.Settings.FirstOrDefaultAsync(_ => _.Code == code && _.Category == "Count");
        if (count is null)
        {
            return value;
        }
        return (double)count.Value;
    }

    public async Task<(Grade? prev, Grade? next)> GetRelationAsync(Grade current)
    {
        var next = await _context.GradeRelations.Where(_ => _.Start == current).Select(_ => _.End).FirstOrDefaultAsync();
        var prev = await _context.GradeRelations.Where(_ => _.End == current).Select(_ => _.Start).FirstOrDefaultAsync();
        return (prev, next);
    }
}