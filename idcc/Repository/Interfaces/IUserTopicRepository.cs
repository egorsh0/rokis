using idcc.Dtos;
using idcc.Models;

namespace idcc.Repository.Interfaces;

public interface IUserTopicRepository
{
    Task<bool> HasOpenTopic(Session session);
    Task<UserTopicDto?> GetRandomTopicAsync(Session session);
    Task<UserTopicDto?> GetActualTopicAsync(Session session);
    Task<UserTopicDto?> GetTopicAsync(int id);
    Task<List<UserTopicDto>> GetAllTopicsAsync(SessionDto session);
    Task UpdateTopicInfoAsync(int id, bool actual, bool previous, GradeDto? grade = null, double? weight = null);
    Task ReduceTopicQuestionCountAsync(int id);
    Task RefreshActualTopicInfoAsync(int id, Session session);
    Task CloseTopicAsync(int id);
    Task<int?> CountQuestionAsync(int id, double max);
}