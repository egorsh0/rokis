using idcc.Models;

namespace idcc.Repository.Interfaces;

public interface IUserTopicRepository
{
    Task<bool> HasOpenTopic(Session session);
    Task<UserTopic?> GetRandomTopicAsync(Session session);
    Task<UserTopic?> GetActualTopicAsync(Session session);
    Task<UserTopic?> GetTopicAsync(int id);
    Task<List<UserTopic>> GetAllTopicsAsync(Session session);
    Task UpdateTopicInfoAsync(int id, bool actual, bool previous, Grade? grade = null, double? weight = null);
    Task ReduceTopicQuestionCountAsync(int id);
    Task RefreshActualTopicInfoAsync(int id, Session session);
    Task CloseTopicAsync(int id);
    Task<int?> CountQuestionAsync(int id, double max);
}