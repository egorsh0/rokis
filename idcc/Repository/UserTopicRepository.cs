using idcc.Context;
using idcc.Dtos;
using idcc.Models;
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

    public async Task<bool> HasOpenTopic(Session session)
    {
        return await _context.UserTopics.AnyAsync(t => t.IsFinished == false && t.Session == session);
    }

    public async Task<UserTopicDto?> GetRandomTopicAsync(Session session)
    {
        var userTopics = await _context.UserTopics.Where(t => t.IsFinished == false && t.Session == session)
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
    
    public async Task<UserTopicDto?> GetActualTopicAsync(Session session)
    {
        var userTopic = await _context.UserTopics
            .Where(t => t.IsFinished == false && t.Actual == true && t.Session == session)
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

    public async Task<List<UserTopicDto>> GetAllTopicsAsync(Session session)
    {
        var userTopics = await _context.UserTopics
            .Where(t => t.Session == session)
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

    public async Task RefreshActualTopicInfoAsync(int id, Session session)
    {
        var userTopics = await _context.UserTopics.Where(t => t.Session == session).ToListAsync();
        foreach (var userTopic in userTopics)
        {
            if (userTopic.Id == id)
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

    public async Task<int?> CountQuestionAsync(int id, double max)
    {
        var userTopic = await _context.UserTopics.Where(t => t.Id == id).FirstOrDefaultAsync();
        if (userTopic is null)
        {
            return null;
        }

        var actualWeight = userTopic.Weight;
        var count = await _context.Questions.CountAsync(q =>
            q.Topic == userTopic.Topic && q.Weight > actualWeight && q.Weight < max);
        return count;
    }
}