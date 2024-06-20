using idcc.Context;
using idcc.Models;
using idcc.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace idcc.Repository;

public class UserGradeRepository : IUserGradeRepository
{
    private readonly IdccContext _context;

    private readonly ILogger<UserGradeRepository> _logger;

    public UserGradeRepository(IdccContext context, ILogger<UserGradeRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public UserGrade GetActualUserDataAsync(int userId)
    {
        var userGrade = _context.UserGrades.Where(_ => _.IsFinished == false && _.User.Id == userId).OrderBy(o => Guid.NewGuid()).First();
        return userGrade;
    }

    public async Task UpdateScoreAsync(int userGradeId, double score)
    {
        var userGrade = await _context.UserGrades.FindAsync(userGradeId);
        var oldScore = userGrade.Score;
        userGrade.Score = oldScore + score;
        _logger.LogInformation("Old score = " + oldScore);
        _logger.LogInformation("New score = " + userGrade.Score);
        await _context.SaveChangesAsync();
    }
    
    public async Task UpdateFinishedAsync(int userGradeId)
    {
        var userGrade = await _context.UserGrades.FindAsync(userGradeId);
        userGrade.IsFinished = true;
        await _context.SaveChangesAsync();
    }

    public async Task<double?> CalculateAsync(int userId)
    {
        var checkUserGrade = await _context.UserGrades.AllAsync(_ => _.User.Id == userId && _.IsFinished == true);
        if (!checkUserGrade)
        {
            return null;
        }

        var userGrades = await _context.UserGrades.Where(_ => _.User.Id == userId).ToListAsync();
        var scores = userGrades.Select(userGrade => userGrade.Score).ToList();
        return scores.Average();
    }
}