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
        var answerTime = await _context.AnswerTimes.Where(time => time.Grade.Id == gradeId).FirstOrDefaultAsync();
        if (answerTime is null)
        {
            return null;
        }

        return (answerTime.Average, answerTime.Min, answerTime.Max);
    }

    public async Task<(double min, double max)?> GetGradeWeightInfoAsync(int gradeId)
    {
        var weight = await _context.Weights.Where(w => w.Grade.Id == gradeId).FirstOrDefaultAsync();
        if (weight is null)
        {
            return null;
        }

        return (weight.Min, weight.Max);
    }
    
    public async Task<(double, Grade)> GetGradeLevelAsync(double score)
    {
        var gradeLevel = await _context.GradeLevels.Select(l => l.Level).ToListAsync();
        var value = gradeLevel.MinBy(n => Math.Abs(n - score));
        
        var grade = await _context.GradeLevels.Where(level => level.Level.Equals(value)).Include(l => l.Grade)
            .FirstAsync();
        return (value, grade.Grade);
    }

    public async Task<double> GetPercentOrDefaultAsync(string code, double value)
    {
        var percent = await _context.Persents.FirstOrDefaultAsync(p => p.Code == code);
        return percent?.Value ?? value;
    }
    
    public async Task<int> GetCountOrDefaultAsync(string code, int value)
    {
        var count = await _context.Counts.FirstOrDefaultAsync(c => c.Code == code);
        return count?.Value ?? value;
    }

    public async Task<(Grade? prev, Grade? next)> GetRelationAsync(Grade current)
    {
        var next = await _context.GradeRelations.Where(r => r.Start == current).Select(x => x.End).FirstOrDefaultAsync();
        var prev = await _context.GradeRelations.Where(r => r.End == current).Select(x => x.Start).FirstOrDefaultAsync();
        return (prev, next);
    }
}