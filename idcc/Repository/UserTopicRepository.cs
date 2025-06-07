using idcc.Context;
using idcc.Dtos;
using idcc.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace idcc.Repository;

public class UserTopicRepository : IUserTopicRepository
{
    private readonly IdccContext _context;

    public UserTopicRepository(IdccContext context)
    {
        _context = context;
    }

    public async Task<bool> HasOpenTopic(int sessionId)
    {
        return await _context.UserTopics.AnyAsync(t => t.IsFinished == false && t.Session.Id == sessionId);
    }

    public async Task<UserTopicDto?> GetRandomTopicAsync(int sessionId)
    {
        var userTopics = await _context.UserTopics
            .Where(t => t.IsFinished == false && t.Session.Id == sessionId)
            .Include(userTopic => userTopic.Session).Include(userTopic => userTopic.Topic)
            .ThenInclude(topic => topic.Direction)
            .Include(userTopic => userTopic.Grade).ToListAsync();
        if (!userTopics.Any())
        {
            return null;
        }
        if (userTopics.Count == 1)
        {
            var ut =  userTopics.First();
            return new UserTopicDto(ut.Id, ut.Session.Id, new TopicDto(ut.Topic.Id, ut.Topic.Name, ut.Topic.Description, ut.Topic.Direction.Id), new GradeDto(ut.Grade.Id, ut.Grade.Name, ut.Grade.Code, ut.Grade.Description), ut.Weight, ut.IsFinished, ut.WasPrevious, ut.Actual, ut.Count);
        }
        var userTopic = userTopics.Where(t => t.WasPrevious == false).MinBy(_ => Guid.NewGuid());
        return userTopic == null ? null : new UserTopicDto(userTopic.Id, userTopic.Session.Id,
            new TopicDto(userTopic.Topic.Id, userTopic.Topic.Name, userTopic.Topic.Description, userTopic.Topic.Direction.Id),
            new GradeDto(userTopic.Grade.Id, userTopic.Grade.Name, userTopic.Grade.Code, userTopic.Grade.Description),
            userTopic.Weight, userTopic.IsFinished, userTopic.WasPrevious, userTopic.Actual, userTopic.Count);
    }
    
    public async Task<UserTopicDto?> GetActualTopicAsync(int sessionId)
    {
        var userTopic = await _context.UserTopics
            .Where(t => t.IsFinished == false && t.Actual && t.Session.Id == sessionId)
            .Include(userTopic => userTopic.Session).Include(userTopic => userTopic.Topic)
            .ThenInclude(topic => topic.Direction)
            .Include(userTopic => userTopic.Grade).FirstOrDefaultAsync();
        
        return userTopic == null ? null : new UserTopicDto(userTopic.Id, userTopic.Session.Id,
            new TopicDto(userTopic.Topic.Id, userTopic.Topic.Name, userTopic.Topic.Description, userTopic.Topic.Direction.Id),
            new GradeDto(userTopic.Grade.Id, userTopic.Grade.Name, userTopic.Grade.Code, userTopic.Grade.Description),
            userTopic.Weight, userTopic.IsFinished, userTopic.WasPrevious, userTopic.Actual, userTopic.Count);
    }

    public async Task<UserTopicDto?> GetTopicAsync(int id)
    {
        var userTopic = await _context.UserTopics
            .Where(t => t.Id == id)
            .Include(userTopic => userTopic.Session)
            .Include(userTopic => userTopic.Topic)
            .ThenInclude(topic => topic.Direction)
            .Include(userTopic => userTopic.Grade)
            .FirstOrDefaultAsync();
        return userTopic == null ? null : new UserTopicDto(userTopic.Id, userTopic.Session.Id,
            new TopicDto(userTopic.Topic.Id, userTopic.Topic.Name, userTopic.Topic.Description, userTopic.Topic.Direction.Id),
            new GradeDto(userTopic.Grade.Id, userTopic.Grade.Name, userTopic.Grade.Code, userTopic.Grade.Description),
            userTopic.Weight, userTopic.IsFinished, userTopic.WasPrevious, userTopic.Actual, userTopic.Count);
    }

    public async Task<List<UserTopicDto>> GetAllTopicsAsync(int sessionId)
    {
        var userTopics = await _context.UserTopics
            .Where(t => t.Session.Id == sessionId)
            .Include(userTopic => userTopic.Session)
            .Include(userTopic => userTopic.Topic)
            .ThenInclude(topic => topic.Direction)
            .Include(userTopic => userTopic.Grade)
            .Select(userTopic => new UserTopicDto(userTopic.Id, userTopic.Session.Id,
                new TopicDto(userTopic.Topic.Id, userTopic.Topic.Name, userTopic.Topic.Description, userTopic.Topic.Direction.Id),
                new GradeDto(userTopic.Grade.Id, userTopic.Grade.Name, userTopic.Grade.Code, userTopic.Grade.Description),
                userTopic.Weight, userTopic.IsFinished, userTopic.WasPrevious, userTopic.Actual, userTopic.Count))
            .ToListAsync();
        return userTopics;
    }

    public async Task UpdateTopicInfoAsync(int id, bool actual, bool previous, GradeDto? grade, double? weight = null)
    {
        var userTopic = await _context.UserTopics
            .Where(t => t.Id == id)
            .Include(userTopic => userTopic.Grade)
            .FirstOrDefaultAsync();
        if (userTopic is not null)
        {
            userTopic.Actual = actual;
            userTopic.WasPrevious = previous;
            if (weight is not null)
            {
                userTopic.Weight = weight.Value;
            }
            if (grade is not null)
            {
                var prev = await _context.Grades.FindAsync(grade.Id);
                if (prev is not null)
                {
                    userTopic.Grade = prev;
                }
            }
            
            await _context.SaveChangesAsync();
        }
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
        var userTopics = await _context.UserTopics
            .Where(t => t.Session.Id == sessionId)
            .ToListAsync();
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

    public async Task CloseTopicAsync(int id)
    {
        var userTopic = await _context.UserTopics.Where(t => t.Id == id).FirstOrDefaultAsync();
        if (userTopic is not null)
        {
            userTopic.IsFinished = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> HaveQuestionAsync(int topicId, double gradeMax)
    {
        var userTopic = await _context.UserTopics
            .Where(t => t.Id == topicId)
            .FirstOrDefaultAsync();
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