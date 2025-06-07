using idcc.Dtos;
using idcc.Models;

namespace idcc.Repository.Interfaces;

public interface IUserAnswerRepository
{
    Task CreateUserAnswerAsync(SessionDto session, Question question, int timeSpent,
        double score, DateTime answerTime);
    
    Task<List<UserAnswer>> GetAllUserAnswers(int sessionId);
    Task<List<UserAnswer>> GetAllUserAnswers(int sessionId, int topicId);
    Task<List<QuestionResultDto>> GetQuestionResults(SessionDto session);

    Task<bool> CanRaiseAsync(int sessionId, int count);
    Task<bool> CanCloseAsync(int sessionId, int count);
}