using idcc.Context;
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
        return await _context.UserTopics.AnyAsync(_ => _.IsFinished == false && _.Session == session);
    }

    public async Task<UserTopic?> GetRandomTopicAsync(Session session)
    {
        var userTopic = await _context.UserTopics.Where(_ => _.IsFinished == false && _.WasPrevious == false && _.Session == session).OrderBy(o => Guid.NewGuid()).FirstOrDefaultAsync();
        return userTopic;
    }
    
    public async Task<UserTopic?> GetActualTopicAsync(Session session)
    {
        var userTopic = await _context.UserTopics.Where(_ => _.IsFinished == false && _.Actual == true && _.Session == session).FirstOrDefaultAsync();
        return userTopic;
    }

    public async Task<UserTopic?> GetTopicAsync(int id)
    {
        var userTopic = await _context.UserTopics.Where(_ => _.Id == id).FirstOrDefaultAsync();
        return userTopic;
    }

    public async Task<List<UserTopic>> GetAllTopicsAsync(Session session)
    {
        var userTopics = await _context.UserTopics.Where(_ => _.Session == session).ToListAsync();
        return userTopics;
    }

    public async Task UpdateTopicInfoAsync(int id, bool actual, bool previous, Grade? grade, double? weight = null)
    {
        var userTopic = await _context.UserTopics.Where(_ => _.Id == id).FirstOrDefaultAsync();
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
                userTopic.Grade = grade;
            }
            
            await _context.SaveChangesAsync();
        }
    }

    public async Task ReduceTopicQuestionCountAsync(int id)
    {
        var userTopic = await _context.UserTopics.Where(_ => _.Id == id).FirstOrDefaultAsync();
        if (userTopic is not null)
        {
            var count = userTopic.Count;
            userTopic.Count = count - 1;
            await _context.SaveChangesAsync();
        }
    }

    public async Task RefreshActualTopicInfoAsync(int id, Session session)
    {
        var userTopics = await _context.UserTopics.Where(_ => _.Session == session).ToListAsync();
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
        var userTopic = await _context.UserTopics.Where(_ => _.Id == id).FirstOrDefaultAsync();
        if (userTopic is not null)
        {
            userTopic.IsFinished = true;
            await _context.SaveChangesAsync();
        }
    }
}