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
        var gradeLevel = await _context.GradeLevels.Select(_ => _.Level).ToListAsync();
        var value = gradeLevel.OrderBy(x => x)
            .ThenBy(x => Math.Abs(x - score))
            .ElementAt(0);
        
        var grade = await _context.GradeLevels.Where(_ => _.Level.Equals(value)).FirstAsync();
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

    public async Task<List<ISetting>> GetSettingsAsync(SettingType type)
    {
        switch (type)
        {
            case SettingType.AnswerTime:
                
                var answerTimes = await _context.AnswerTimes.Select(_ => new AnswerTime()
                {
                    Id = _.Id,
                    Average = _.Average,
                    Grade = _.Grade.Name,
                    Min = _.Min,
                    Max = _.Max
                }).ToListAsync();

                return new List<ISetting>(answerTimes);
            case SettingType.Count:
                var counts = await _context.Counts.Select(_ => new Count()
                {
                    Id = _.Id,
                    Code = _.Code,
                    Description = _.Description,
                    Value = _.Value
                }).ToListAsync();
                return new List<ISetting>(counts);
            case SettingType.GradeLevel:
                var gradeLevels = await _context.GradeLevels.Select(_ => new GradeLevel()
                {
                    Id = _.Id,
                    Grade = _.Grade.Name,
                    Level = _.Level
                }).ToListAsync();
                return new List<ISetting>(gradeLevels);
            case SettingType.Persent:
                var persents = await _context.Persents.Select(_ => new Persent()
                {
                    Id = _.Id,
                    Code = _.Code,
                    Description = _.Description,
                    Value = _.Value
                }).ToListAsync();
                return new List<ISetting>(persents);
            case SettingType.Weight:
                var weights = await _context.Weights.Select(_ => new Weight()
                {
                    Id = _.Id,
                    Grade = _.Grade.Name,
                    Min = _.Min,
                    Max = _.Max
                }).ToListAsync();
                return new List<ISetting>(weights);
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}