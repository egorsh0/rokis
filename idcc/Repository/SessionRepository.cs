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
    
    public async Task StartSessionAsync(User user)
    {
        var session = new Session()
        {
            User = user,
            Score = 0,
            StartTime = DateTime.Now,
            EndTime = null
        };
        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();
    }

    public async Task EndSessionAsync(User user)
    {
        var session = await _context.Sessions.FirstOrDefaultAsync(_ => _.User == user);
        if (session is not null)
        {
            session.EndTime = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Session?> GetSessionAsync(int userId)
    {
        return await _context.Sessions.FirstOrDefaultAsync(_ => _.User.Id == userId);
    }
}