using idcc.Models;

namespace idcc.Repository.Interfaces;

public interface IUserAnswerRepository
{
    Task CreateUserAnswerAsync(Session session, Question question, int timeSpent,
        double score, DateTime answerTime);

    Task<bool> CanRaiseAsync(int sessionId, int count);
}