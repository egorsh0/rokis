using idcc.Context;
using idcc.Models;
using idcc.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace idcc.Repository;

public class UserAnswerRepository : IUserAnswerRepository
{
    private readonly IdccContext _context;

    public UserAnswerRepository(IdccContext context)
    {
        _context = context;
    }
    
    public async Task CreateUserAnswerAsync(Session session, Question question, int timeSpent, double score,
        DateTime answerTime)
    {
        var userAnswer = new UserAnswer()
        {
            Question = question,
            Session = session,
            Score = score,
            AnswerTime = answerTime,
            TimeSpent = timeSpent
        };
        await _context.UserAnswers.AddAsync(userAnswer);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> CanRaiseAsync(int sessionId, int count)
    {
        var userAnswers = await _context.UserAnswers.Where(_ => _.Session.Id == sessionId).OrderByDescending(_ => _.AnswerTime).ToListAsync();
        var correctCount = count;
        foreach (var userAnswer in userAnswers)
        {
            bool hasCorrect;
            if (userAnswer.Score > 0)
            {
                correctCount--;
                hasCorrect = true;
            }
            else
            {
                return false;
            }

            switch (hasCorrect)
            {
                case false when correctCount > 0:
                    return false;
                case true when correctCount == 0:
                    return true;
            }
        }

        return false;
    }
}