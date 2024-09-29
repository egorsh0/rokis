using idcc.Models;

namespace idcc.Application.Interfaces;

public interface IIdccApplication
{
    Task<string?> CalculateScoreAsync(Session session, int interval, int questionId, List<int> answerIds);
    
    Task<string?> CalculateTopicWeightAsync(Session session);
}