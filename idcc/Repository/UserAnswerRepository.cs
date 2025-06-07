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
    
    public async Task CreateUserAnswerAsync(SessionDto sessionDto, Question question, int timeSpent, double score,
        DateTime answerTime)
    {
        var session = await _context.Sessions.FindAsync(sessionDto.Id);
        if (session == null)
        {
            return;
        }
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

    public async Task<List<UserAnswer>> GetAllUserAnswers(int sessionId)
    {
        return await _context.UserAnswers.Where(a => a.Session.Id == sessionId).ToListAsync();
    }
    
    public async Task<List<UserAnswer>> GetAllUserAnswers(int sessionId, int topicId)
    {
        return await _context.UserAnswers.Where(a => 
            a.Session.Id == sessionId && 
            a.Question.Topic.Id == topicId)
            .ToListAsync();
    }
    
    public async Task<List<QuestionResultDto>> GetQuestionResults(SessionDto session)
    {
        var userAnswers = await _context.UserAnswers.Where(a => 
                a.Session.Id == session.Id)
            .Include(userAnswer => userAnswer.Question).ToListAsync();

        return userAnswers.Select(userAnswer => new QuestionResultDto(userAnswer.Question.Weight, userAnswer.Score > 0, userAnswer.TimeSpent)).ToList();
    }

    public async Task<bool> CanRaiseAsync(int sessionId, int count)
    {
        var userAnswers = await _context.UserAnswers
            .Where(a => a.Session.Id == sessionId)
            .OrderByDescending(x => x.AnswerTime)
            .ToListAsync();
        
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
    
    public async Task<bool> CanCloseAsync(int sessionId, int count)
    {
        var userAnswers = await _context.UserAnswers
            .Where(a => a.Session.Id == sessionId)
            .OrderByDescending(x => x.AnswerTime)
            .ToListAsync();

        var uncorrectCount = 0;
        foreach (var userAnswer in userAnswers)
        {
            if (userAnswer.Score == 0)
            {
                uncorrectCount++;
                if (uncorrectCount == count)
                {
                    return true;
                }
            }
            else
            {
                uncorrectCount = 0;
            }
        }

        return false;
    }
}