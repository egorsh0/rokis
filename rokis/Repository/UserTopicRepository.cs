using rokis.Context;
using rokis.Models;
using Microsoft.EntityFrameworkCore;

namespace rokis.Repository;

public interface IUserTopicRepository
{
    /// <summary>
    /// Получение темы пользователя.
    /// </summary>
    /// <param name="userTopicId">Идентификатор темы пользователя.</param>
    /// <returns></returns>
    Task<UserTopic?> GetUserTopicAsync(int userTopicId);
    
    /// <summary>
    /// Получение тем пользователя.
    /// </summary>
    /// <param name="sessionId">Идентификатор сессии.</param>
    /// <param name="isFinished">Закончена?</param>
    /// <returns></returns>
    Task<List<UserTopic>> GetUserTopicsAsync(int sessionId, bool isFinished = false);
    
    /// <summary>
    /// Создание пользовательского топика.
    /// </summary>
    /// <returns></returns>
    Task CreateUserTopicAsync(Session session, Topic topic, double weightMin, Grade grade, bool b, bool b1, bool firstActual, int questionCount);

    /// <summary>
    /// Обновление информации пользовательской темы.
    /// </summary>
    /// <param name="userTopic">Идентификатор темы.</param>
    /// <returns></returns>
    Task UpdateTopicInfoAsync(UserTopic userTopic);
    
    /// <summary>
    /// Изменение актуальной пользовательской темы.
    /// </summary>
    /// <param name="userTopicId">Идентификатор темы.</param>
    /// <param name="sessionId">Идентификатор сессии.</param>
    /// <returns></returns>
    Task RefreshActualTopicInfoAsync(int userTopicId, int sessionId);
    
    /// <summary>
    /// Закрытие пользовательской темы.
    /// </summary>
    /// <param name="userTopicId">Идентификатор темы.</param>
    /// <returns></returns>
    Task CloseTopicAsync(int userTopicId);
    
    Task<bool> HaveQuestionAsync(int userTopicId, double max);
}

public class UserTopicRepository : IUserTopicRepository
{
    private readonly RokisContext _context;

    public UserTopicRepository(RokisContext context)
    {
        _context = context;
    }

    public async Task<UserTopic?> GetUserTopicAsync(int userTopicId)
    {
        return await _context.UserTopics.FindAsync(userTopicId);
    }

    public async Task<List<UserTopic>> GetUserTopicsAsync(int sessionId, bool isFinished = false)
    {
        return await _context.UserTopics
            .Where(t => t.IsFinished == isFinished && t.Session.Id == sessionId)
            .Include(userTopic => userTopic.Session)
            .Include(userTopic => userTopic.Topic)
            .ThenInclude(topic => topic.Direction)
            .Include(userTopic => userTopic.Grade)
            .ToListAsync();
    }

    public async Task CreateUserTopicAsync(
        Session session,
        Topic topic,
        double weightMin,
        Grade grade,
        bool isFinished,
        bool wasPrevious,
        bool firstActual,
        int questionCount)
    {
        var userTopic = new UserTopic()
        {
            Session = session,
            Topic = topic,
            Weight = weightMin,
            Grade = grade,
            IsFinished = false,
            WasPrevious = false,
            Actual = firstActual,
            Count = questionCount
        };
        _context.UserTopics.Add(userTopic);
        await _context.SaveChangesAsync();
    }
    

    public async Task UpdateTopicInfoAsync(UserTopic userTopic)
    {
        var entity = await _context.UserTopics.FindAsync(userTopic.Id);
        if (entity == null)
        {
            return;
        }
        entity.Topic = userTopic.Topic;
        entity.Weight = userTopic.Weight;
        entity.Grade = userTopic.Grade;
        entity.IsFinished = userTopic.IsFinished;
        entity.WasPrevious = userTopic.WasPrevious;
        entity.Actual = userTopic.Actual;
        entity.Count = userTopic.Count;
        
        await _context.SaveChangesAsync();
    }

    public async Task ReduceTopicQuestionCountAsync(int id)
    {
        var userTopic = await _context.UserTopics.Where(t => t.Id == id).FirstOrDefaultAsync();
        if (userTopic is not null)
        {
            var count = userTopic.Count;
            userTopic.Count = count - 1;
            await _context.SaveChangesAsync();
        }
    }

    public async Task RefreshActualTopicInfoAsync(int userTopicId, int sessionId)
    {
        var userTopics = await GetUserTopicsAsync(sessionId);
        foreach (var userTopic in userTopics)
        {
            if (userTopic.Id == userTopicId)
            {
                userTopic.Actual = true;
                userTopic.WasPrevious = true;
            }
            else
            {
                userTopic.Actual = false;
                userTopic.WasPrevious = false;
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task CloseTopicAsync(int userTopicId)
    {
        var userTopic = await _context.UserTopics.Where(t => t.Id == userTopicId).FirstOrDefaultAsync();
        if (userTopic is not null)
        {
            userTopic.IsFinished = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> HaveQuestionAsync(int userTopicId, double gradeMax)
    {
        var userTopic = await GetUserTopicAsync(userTopicId);
        if (userTopic is null)
        {
            return false;
        }

        var actualWeight = userTopic.Weight;
        return await _context.Questions
            .AnyAsync(q =>
                q.Topic == userTopic.Topic && 
                q.Weight >= actualWeight && 
                q.Weight <= gradeMax);
    }
}