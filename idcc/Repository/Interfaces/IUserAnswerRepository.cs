using idcc.Dtos;
using idcc.Models;

namespace idcc.Repository.Interfaces;

public interface IUserAnswerRepository
{
    Task CreateUserAnswerAsync(Session session, Question question, int timeSpent,
        double score, DateTime answerTime);
    
    Task<List<UserAnswer>> GetAllUserAnswers(Session session);
    Task<List<QuestionResultDto>> GetQuestionResults(Session session);

    Task<bool> CanRaiseAsync(Session session, int count);
}