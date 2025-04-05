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
    
    public async Task<Session> StartSessionAsync(Employee employee, Role role)
    {
        var session = new Session()
        {
            Employee = employee,
            Score = 0,
            StartTime = DateTime.Now,
            EndTime = null,
            Role = role
        };
        _context.Sessions.Add(session);

        await CreateSessionUserTopics(session);
        
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task<bool> EndSessionAsync(int id, bool faster)
    {
        var session = await _context.Sessions.FindAsync(id);
        if (session is null)
        {
            return false;
        }
        session.EndTime = DateTime.Now;
        if (faster)
        {
            session.Score = -1;
        }
        else
        {
            var userTopics = await _context.UserTopics.Where(t => t.Session == session && t.IsFinished == false).ToListAsync();
            foreach (var userTopic in userTopics)
            {
                userTopic.IsFinished = true;
            }
        }
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Session?> GetSessionAsync(int id)
    {
        return await _context.Sessions.FindAsync(id);
    }

    public async Task<List<Session>> GetSessionsAsync(Employee employee)
    {
        return await _context.Sessions.Where(s => s.Employee == employee).ToListAsync();
    }

    public async Task<Session?> GetActualSessionAsync(string name)
    {
        var where = _context.Sessions.Where(s => s.Employee.Name == name && s.EndTime == null);
        return await _context.Sessions.SingleOrDefaultAsync(s => s.Employee.Name == name && s.EndTime == null);
    }

    public async Task<Session?> GetFinishSessionAsync(string name)
    {
        return await _context.Sessions.Where(s => s.Employee.Name == name && s.Score >= 0).OrderByDescending(s => s.EndTime).FirstOrDefaultAsync();
    }

    public async Task SessionScoreAsync(int id, double score)
    {
        var session = await _context.Sessions.FindAsync(id);
        if (session is not null)
        {
            session.Score = score;
        }
        await _context.SaveChangesAsync();
    }

    private async Task CreateSessionUserTopics(Session session)
    {
        var middleGrade = _context.Grades.Single(g => g.Code == "Middle");
        var weight = _context.Weights.Single(w => w.Grade == middleGrade);
        var topics = _context.Topics.Where(t => t.Role == session.Role);
        var settingQuestion = await _context.Counts.FirstOrDefaultAsync(c => c.Code == "Question");

        var questionCount = 10;
        if (settingQuestion is not null)
        {
            questionCount = settingQuestion.Value;
        }

        bool firstActual = true;
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
                Actual = firstActual,
                Count = questionCount
            };
            firstActual = false;
            _context.UserTopics.Add(userTopic);
        }
    }
}