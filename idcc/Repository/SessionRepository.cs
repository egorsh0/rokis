using idcc.Context;
using idcc.Models;
using idcc.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace idcc.Repository;

public class SessionRepository : ISessionRepository
{
    private readonly IdccContext _context;

    public SessionRepository(IdccContext context)
    {
        _context = context;
    }
    
    public async Task<Session> StartSessionAsync(User user)
    {
        var session = new Session()
        {
            User = user,
            Score = 0,
            StartTime = DateTime.Now,
            EndTime = null
        };
        _context.Sessions.Add(session);

        await CreateSessionUserTopics(session);
        
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task EndSessionAsync(int id)
    {
        var session = await _context.Sessions.FindAsync(id);
        if (session is not null)
        {
            session.EndTime = DateTime.Now;

            var userTopics = await _context.UserTopics.Where(_ => _.Session == session && _.IsFinished == false).ToListAsync();
            foreach (var userTopic in userTopics)
            {
                userTopic.IsFinished = true;
            }
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Session?> GetSessionAsync(int id)
    {
        return await _context.Sessions.FindAsync(id);
    }

    private async Task CreateSessionUserTopics(Session session)
    {
        var middleGrade = _context.Grades.Single(_ => _.Code == "Middle");
        var weight = _context.Weights.Single(_ => _.Grade == middleGrade);
        var topics = _context.Topics.Where(_ => _.Role == session.User.Role);
        var settingQuestion = await _context.Counts.FirstOrDefaultAsync(_ => _.Code == "Question");

        var questionCount = 10;
        if (settingQuestion is not null)
        {
            questionCount = settingQuestion.Value;
        }

        foreach (var topic in topics)
        {
            var userTopic = new UserTopic()
            {
                Session = session,
                Topic = topic,
                Weight = weight.Min,
                Grade = middleGrade,
                IsFinished = false,
                WasPrevious = false,
                Actual = false,
                Count = questionCount
            };
            _context.UserTopics.Add(userTopic);
        }
    }
}