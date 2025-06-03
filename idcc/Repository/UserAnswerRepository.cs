using idcc.Context;
using idcc.Dtos;
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

    public async Task<List<UserAnswer>> GetAllUserAnswers(Session session)
    {
        return await _context.UserAnswers.Where(a => a.Session == session).ToListAsync();
    }
    
    public async Task<List<QuestionResultDto>> GetQuestionResults(Session session)
    {
        var userAnswers = await _context.UserAnswers.Where(a => a.Session == session)
            .Include(userAnswer => userAnswer.Question).ToListAsync();

        return userAnswers.Select(userAnswer => new QuestionResultDto(userAnswer.Question.Weight, userAnswer.Score > 0, userAnswer.TimeSpent)).ToList();
    }

    public async Task<bool> CanRaiseAsync(Session session, int count)
    {
        var userAnswers = await _context.UserAnswers.Where(a => a.Session == session).OrderByDescending(x => x.AnswerTime).ToListAsync();
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