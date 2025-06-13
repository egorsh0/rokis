using rokis.Context;
using rokis.Dtos;
using rokis.Models;
using Microsoft.EntityFrameworkCore;

namespace rokis.Repository;

public interface IUserAnswerRepository
{
    /// <summary>
    /// Список ответов пользователя в рамках одной сессии и одной темы,
    /// упорядоченный от новых к старым.
    /// </summary>
    Task<List<UserAnswer>> GetUserAnswersAsync(int sessionId, int topicId);
    
    Task CreateUserAnswerAsync(SessionDto session, QuestionSmartDto question, int timeSpent,
        double score, DateTime answerTime);
    
    Task<List<UserAnswer>> GetAllUserAnswers(int sessionId);
    Task<List<UserAnswer>> GetAllUserAnswers(int sessionId, int topicId);
    Task<List<QuestionResultDto>> GetQuestionResults(SessionDto session);
}

public class UserAnswerRepository : IUserAnswerRepository
{
    private readonly RokisContext _context;

    public UserAnswerRepository(
        RokisContext context)
    {
        _context = context;
    }
    
    /// <inheritdoc />
    public async Task<List<UserAnswer>> GetUserAnswersAsync(int sessionId, int topicId)
    {
        return await _context.UserAnswers
            .AsNoTracking()
            .Include(a => a.Question)
            .Where(a => a.Session.Id == sessionId &&
                        a.Question.Topic.Id == topicId)
            .OrderByDescending(a => a.AnswerTime)
            .ToListAsync();
    }

    public async Task CreateUserAnswerAsync(SessionDto sessionDto, QuestionSmartDto question, int timeSpent, double score,
        DateTime answerTime)
    {
        var session = await _context.Sessions.FindAsync(sessionDto.Id);
        if (session == null)
        {
            return;
        }

        var entity = await _context.Questions.FindAsync(question.QuestionId);
        if (entity == null)
        {
            return;
        }
        
        var userAnswer = new UserAnswer()
        {
            Question = entity,
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
}