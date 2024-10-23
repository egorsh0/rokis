using idcc.Context;
using idcc.Models.AdminDto;
using idcc.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace idcc.Repository;

public class SettingsRepository : ISettingsRepository
{
    private readonly IdccContext _context;

    public SettingsRepository(IdccContext context)
    {
        _context = context;
    }
    
    public async Task<List<AnswerTime>> GetAnswerTimesAsync()
    {
        var answerTimes = await _context.AnswerTimes.Select(_ => new AnswerTime()
        {
            Id = _.Id,
            Average = _.Average,
            Grade = _.Grade.Name,
            Min = _.Min,
            Max = _.Max
        }).ToListAsync();

        return new List<AnswerTime>(answerTimes);
    }

    public async Task<List<Count>> GetCountsAsync()
    {
        var counts = await _context.Counts.Select(_ => new Count()
        {
            Id = _.Id,
            Code = _.Code,
            Description = _.Description,
            Value = _.Value
        }).ToListAsync();
        return new List<Count>(counts);
    }

    public async Task<List<GradeLevel>> GetGradeLevelsAsync()
    {
        var gradeLevels = await _context.GradeLevels.Select(_ => new GradeLevel()
        {
            Id = _.Id,
            Grade = _.Grade.Name,
            Level = _.Level
        }).ToListAsync();
        return new List<GradeLevel>(gradeLevels);
    }

    public async Task<List<Persent>> GetPersentsAsync()
    {
        var persents = await _context.Persents.Select(_ => new Persent()
        {
            Id = _.Id,
            Code = _.Code,
            Description = _.Description,
            Value = _.Value
        }).ToListAsync();
        return new List<Persent>(persents);
    }

    public async Task<List<Weight>> GetWeightsAsync()
    {
        var weights = await _context.Weights.Select(_ => new Weight()
        {
            Id = _.Id,
            Grade = _.Grade.Name,
            Min = _.Min,
            Max = _.Max
        }).ToListAsync();
        return new List<Weight>(weights);
    }
}