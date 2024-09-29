using idcc.Models;

namespace idcc.Repository.Interfaces;

public interface IUserTopicRepository
{
    Task<bool> HasOpenTopic(int userId);
    Task<UserTopic?> GetRandomTopicAsync(int userId);
    Task<UserTopic?> GetActualTopicAsync(int userId);
    Task<UserTopic?> GetTopicAsync(int id);
    Task UpdateTopicInfoAsync(int id, bool actual, bool previous, Grade? grade = null, double? weight = null);
    Task ReduceTopicQuestionCountAsync(int id);
    Task RefreshActualTopicInfoAsync(int id, int userId);
    Task CloseTopicAsync(int id);
}