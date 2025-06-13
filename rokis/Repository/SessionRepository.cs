using rokis.Context;
using rokis.Infrastructures;
using rokis.Models;
using Microsoft.EntityFrameworkCore;

namespace rokis.Repository;

public interface ISessionRepository
{
    /// <summary>
    /// Получить сессию.
    /// </summary>
    /// <param name="sessionId">Идентификатор сессии.</param>
    /// <returns></returns>
    Task<Session?> GetSessionAsync(int sessionId);
    
    /// <summary>
    /// Получить сессию.
    /// </summary>
    /// <param name="tokenId">Уникальный токен.</param>
    /// <returns></returns>
    Task<Session?> GetSessionAsync(Guid tokenId);
    
    /// <summary>
    /// Получить сессию.
    /// </summary>
    /// <param name="tokenId">Уникальный токен.</param>
    /// <param name="endDate">Дата закрытия сессии.</param>
    /// <returns></returns>
    Task<Session?> GetSessionAsync(Guid tokenId, DateTime? endDate);
    
    /// <summary>
    /// Получить сессии.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="isEmployee">Это сотрудник?</param>
    /// <returns></returns>
    Task<List<Session>> GetSessionsAsync(string userId, bool isEmployee);
    
    /// <summary>
    /// Получить сессии.
    /// </summary>
    /// <param name="directionId">Идентификатор направления.</param>
    /// <returns></returns>
    Task<List<Session>> GetSessionsAsync(int directionId);

    /// <summary>
    /// Создать сессию.
    /// </summary>
    /// <param name="tokenId">Уникальный токен.</param>
    /// <param name="employeeUserId">Идентификатор сотрудника.</param>
    /// <param name="personUserId">Идентификатор физ.лица.</param>
    /// <returns></returns>
    Task<Session> CreateAsync(Guid tokenId, string? employeeUserId, string? personUserId);

    /// <summary>
    /// Завершить сессию.
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    Task CloseSessionAsync(int sessionId);
    
    Task<Session?> GetFinishSessionAsync(Guid tokenId);
    
    Task SessionScoreAsync(int sessionId, double score);
}

public class SessionRepository : ISessionRepository
{
    private readonly RokisContext _context;
    private readonly ITokenRepository _tokenRepository;

    public SessionRepository(
        RokisContext context,
        ITokenRepository tokenRepository)
    {
        _context = context;
        _tokenRepository = tokenRepository;
    }

    public async Task<List<Session>> GetSessionsAsync(string userId, bool isEmployee)
    {
        // базовый запрос
        IQueryable<Session> query = _context.Sessions
            .AsNoTracking()
            .Include(s => s.Token)
            .ThenInclude(t => t.Direction);

        // фильтр по пользователю
        query = isEmployee
            ? query.Where(s => s.EmployeeUserId == userId)
            : query.Where(s => s.PersonUserId   == userId);
        return await query.ToListAsync();
    }

    public async Task<List<Session>> GetSessionsAsync(int directionId)
    {
        IQueryable<Session> query = _context.Sessions
            .AsNoTracking()
            .Include(s => s.Token)
            .ThenInclude(t => t.Direction);
        
        query = query.Where(s => 
            s.Token.Direction.Id == directionId);
        return await query.ToListAsync();
    }

    public async Task<Session> CreateAsync(Guid tokenId, string? employeeUserId, string? personUserId)
    {
        var entity = new Session {
            TokenId = tokenId,
            StartTime = DateTime.UtcNow,
            EmployeeUserId = employeeUserId,
            PersonUserId = personUserId
        };
        _context.Sessions.Add(entity);
        await _tokenRepository.UpdateTokesStatusAsync(tokenId, TokenStatus.Used);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task CloseSessionAsync(int sessionId)
    {
        var session = await _context.Sessions
            .FirstOrDefaultAsync(s => s.Id == sessionId);
        
        if (session is null)
        {
            return;
        }
        session.EndTime = DateTime.Now;
        await _context.SaveChangesAsync();
    }

    public async Task<Session?> GetSessionAsync(int sessionId)
    {
        return await _context.Sessions.FindAsync(sessionId);
    }

    public async Task<Session?> GetSessionAsync(Guid tokenId)
    {
        var sessions = await _context.Sessions.Where(s => s.TokenId == tokenId).ToListAsync();
        if (sessions.Any() && sessions.Count > 1)
        {
            return null;
        }

        if (!sessions.Any())
        {
            return null;
        }
        
        return sessions.FirstOrDefault();
    }

    public async Task<Session?> GetSessionAsync(Guid tokenId, DateTime? endDate)
    {
        var sessions = await _context.Sessions
            .Include(session => session.Token)
            .ThenInclude(token => token.Direction)
            .Where(s => s.TokenId == tokenId && s.EndTime == endDate)
            .ToListAsync();
        if (sessions.Any() && sessions.Count > 1)
        {
            return null;
        }

        if (!sessions.Any())
        {
            return null;
        }
        
        return sessions.FirstOrDefault();
    }

    public async Task<Session?> GetFinishSessionAsync(Guid tokenId)
    {
        return await _context.Sessions.Where(s => s.TokenId == tokenId && s.Score >= 0).OrderByDescending(s => s.EndTime).FirstOrDefaultAsync();
    }

    public async Task SessionScoreAsync(int sessionId, double score)
    {
        // 1. Подгружаем сессию сразу с Token’ом
        var session = await _context.Sessions
            .Include(s => s.Token)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session is null)
        {
            return;
        }

        session.Score = score;
        session.Token.Score = score;

        await _context.SaveChangesAsync();
    }
}