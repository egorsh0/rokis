using idcc.Dtos;
using idcc.Models;

namespace idcc.Repository.Interfaces;

public interface IUserTopicRepository
{
    Task<bool> HasOpenTopic(int sessionId);
    Task<UserTopicDto?> GetRandomTopicAsync(int sessionId);
    Task<UserTopicDto?> GetActualTopicAsync(int sessionId);
    Task<UserTopicDto?> GetTopicAsync(int id);
    Task<List<UserTopicDto>> GetAllTopicsAsync(int sessionId);
    Task UpdateTopicInfoAsync(int id, bool actual, bool previous, GradeDto? grade = null, double? weight = null);
    Task ReduceTopicQuestionCountAsync(int id);
    Task RefreshActualTopicInfoAsync(int userTopicId, int sessionId);
    Task CloseTopicAsync(int id);
    Task<bool> HaveQuestionAsync(int id, double max);
}