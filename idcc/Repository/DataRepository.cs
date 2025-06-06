using idcc.Context;
using idcc.Dtos;
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
        var gradeLevel = await _context.GradeLevels
            .Where(gl => score >= gl.Min && score < gl.Max)
            .Include(gl => gl.Grade)
            .FirstOrDefaultAsync();

        if (gradeLevel == null)
            throw new Exception($"Не удалось определить грейд для score = {score}");

        return (score, gradeLevel.Grade);
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

    public async Task<(GradeDto? prev, GradeDto? next)> GetRelationAsync(GradeDto current)
    {
        var next = await _context.GradeRelations
            .Where(r => r.Start != null && r.Start.Id == current.Id)
            .Select(x => x.End)
            .FirstOrDefaultAsync();

        var prev = await _context.GradeRelations
            .Where(r => r.End != null && r.End.Id == current.Id)
            .Select(x => x.Start)
            .FirstOrDefaultAsync();

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
    }
}