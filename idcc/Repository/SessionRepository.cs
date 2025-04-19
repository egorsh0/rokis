using idcc.Context;
using idcc.Infrastructures;
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
    
    public async Task<Session> StartSessionAsync(string userId, bool isEmployee, Guid tokenId)
    {
        var token = await _context.Tokens.FindAsync(tokenId);
        if(token==null) throw new Exception("Token not found");
        if(token.Status!=TokenStatus.Bound) throw new Exception("Token not bound");

        switch (isEmployee)
        {
            // Проверим, что текущий userId совпадает с тем, кому он привязан
            case true when token.EmployeeUserId!=userId:
            case false when token.PersonUserId!=userId:
                throw new Exception("Forbidden");
        }

        var session = new Session {
            TokenId=tokenId,
            StartTime=DateTime.UtcNow,
            EmployeeUserId = isEmployee?userId:null,
            PersonUserId = isEmployee?null:userId
        };
        _context.Sessions.Add(session);
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
    
    public async Task<IEnumerable<Session>> GetSessionsForUserAsync(string userId, bool isEmployee)
    {
        if(isEmployee)
            return await _context.Sessions
                .Include(s=>s.Token)
                .Where(s=>s.EmployeeUserId==userId)
                .ToListAsync();
        return await _context.Sessions
            .Include(s=>s.Token)
            .Where(s=>s.PersonUserId==userId)
            .ToListAsync();
    }

    public async Task<Session?> GetSessionAsync(int id)
    {
        return await _context.Sessions.FindAsync(id);
    }

    public async Task<Session?> GetActualSessionAsync(Guid tokenId)
    {
        return await _context.Sessions.SingleOrDefaultAsync(s => s.TokenId == tokenId && s.EndTime == null);
    }

    public async Task<Session?> GetFinishSessionAsync(Guid tokenId)
    {
        return await _context.Sessions.Where(s => s.TokenId == tokenId && s.Score >= 0).OrderByDescending(s => s.EndTime).FirstOrDefaultAsync();
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
}