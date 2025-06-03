using idcc.Infrastructures;
using idcc.Models;

namespace idcc.Application.Interfaces;

public interface IIdccApplication
{
    Task<(MessageCode code, string? error)> CalculateScoreAsync(Session session, int interval, int questionId, List<int> answerIds);
    
    Task<(MessageCode code, string? error)> CalculateTopicWeightAsync(Session session);
}