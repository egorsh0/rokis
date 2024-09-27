using idcc.Models;

namespace idcc.Repository.Interfaces;

public interface IUserAnswerRepository
{
    Task CreateUserAnswerAsync(Session session, Question question, List<Answer> answers, int timeSpent,
        double score, DateTime answerTime);

    Task<bool> CanRaiseAsync(int sessionId, double min, int count);
}