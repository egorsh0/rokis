using idcc.Models;

namespace idcc.Application.Interfaces;

public interface IIdccApplication
{
    Task<string?> CalculateScoreAsync(Session session, int userId, int interval, int questionId, IEnumerable<int> answerIds);
    
    Task<string?> CalculateTopicWeightAsync(Session session, int userId);
}