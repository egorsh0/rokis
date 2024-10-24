using idcc.Context;
using idcc.Infrastructures;
using idcc.Models;
using idcc.Models.AdminDto;
using idcc.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using AnswerTime = idcc.Models.AdminDto.AnswerTime;
using Count = idcc.Models.AdminDto.Count;
using GradeLevel = idcc.Models.AdminDto.GradeLevel;
using Persent = idcc.Models.AdminDto.Persent;
using Weight = idcc.Models.AdminDto.Weight;

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
        var gradeLevel = await _context.GradeLevels.Select(l => l.Level).ToListAsync();
        var value = gradeLevel.MinBy(n => Math.Abs(n - score));
        
        var grade = await _context.GradeLevels.Where(_ => _.Level.Equals(value)).Include(gradeLevel => gradeLevel.Grade)
            .FirstAsync();
        return (value, grade.Grade);
    }

    public async Task<double> GetPercentOrDefaultAsync(string code, double value)
    {
        var percent = await _context.Persents.FirstOrDefaultAsync(_ => _.Code == code);
        return percent?.Value ?? value;
    }
    
    public async Task<int> GetCountOrDefaultAsync(string code, int value)
    {
        var count = await _context.Counts.FirstOrDefaultAsync(_ => _.Code == code);
        return count?.Value ?? value;
    }

    public async Task<(Grade? prev, Grade? next)> GetRelationAsync(Grade current)
    {
        var next = await _context.GradeRelations.Where(_ => _.Start == current).Select(_ => _.End).FirstOrDefaultAsync();
        var prev = await _context.GradeRelations.Where(_ => _.End == current).Select(_ => _.Start).FirstOrDefaultAsync();
        return (prev, next);
    }
}