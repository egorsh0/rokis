using idcc.Context;
using idcc.Dtos;
using idcc.Extensions;
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
    
    public async Task<SessionResultDto> StartSessionAsync(string userId, bool isEmployee, Guid tokenId)
    {
        var token = await _context.Tokens
            .Include(t=>t.Order)
            .FirstOrDefaultAsync(t => t.Id == tokenId);

        if (token is null || token.Status != TokenStatus.Bound)
        {
            return new SessionResultDto(null, tokenId, false, MessageCode.TOKEN_NOT_BOUND, MessageCode.TOKEN_NOT_BOUND.GetDescription());
        }

        switch (isEmployee)
        {
            // Проверим, что текущий userId совпадает с тем, кому он привязан
            case true when token.EmployeeUserId != userId:
            case false when token.PersonUserId != userId:
                return new SessionResultDto(null, tokenId, false, MessageCode.TOKEN_IS_FORBIDDEN, MessageCode.TOKEN_IS_FORBIDDEN.GetDescription());
        }
        
        // Проверяем, есть ли уже активная сессия на токен
        var existing = await _context.Sessions
            .FirstOrDefaultAsync(s => s.TokenId == tokenId && s.EndTime == null);
        if (existing != null)
        {
            return new SessionResultDto(existing.Id, existing.TokenId, true, MessageCode.SESSION_HAS_ACTIVE,null);
        } 

        var session = new Session {
            TokenId = tokenId,
            StartTime = DateTime.UtcNow,
            EmployeeUserId = isEmployee ? userId : null,
            PersonUserId   = !isEmployee ? userId : null
        };
        token.Status = TokenStatus.Used;
        _context.Sessions.Add(session);
        await CreateSessionUserTopics(session);
        await _context.SaveChangesAsync();
        return new SessionResultDto(session.Id, session.TokenId, true, MessageCode.SESSION_IS_STARTED, null);
    }

    public async Task<StopSessionDto> EndSessionAsync(Guid tokenId)
    {
        var session = await _context.Sessions
            .FirstOrDefaultAsync(s => s.TokenId == tokenId && s.EndTime == null);
        
        if (session is null)
        {
            return new StopSessionDto(false, MessageCode.SESSION_IS_NOT_EXIST, MessageCode.SESSION_IS_NOT_EXIST.GetDescription());
        }
        session.EndTime = DateTime.Now;
        var userTopics = await _context.UserTopics.Where(t => t.Session.Id == session.Id && t.IsFinished == false).ToListAsync();
        foreach (var userTopic in userTopics)
        {
            userTopic.IsFinished = true;
        }
        
        await _context.SaveChangesAsync();
        return new StopSessionDto(true, MessageCode.SESSION_IS_FINISHED, $"{tokenId} token session completed");
    }
    
    public async Task<IEnumerable<SessionDto>> GetSessionsForUserAsync(
        string userId,
        bool   isEmployee)
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

        // проекция в DTO (циклов больше нет)
        return await query.Select(s => new SessionDto(
                s.Id,
                s.StartTime,
                s.EndTime,
                s.Score,
                new TokenShortDto(
                    s.Token.Id,
                    s.Token.DirectionId,
                    s.Token.Direction.Name,
                    s.Token.Status)))
            .ToListAsync();
    }
    
    public async Task<IEnumerable<SessionDto>> GetCloseSessionsAsync(int directionId)
    {
        IQueryable<Session> query = _context.Sessions
            .AsNoTracking()
            .Include(s => s.Token)
            .ThenInclude(t => t.Direction);
        
        query = query.Where(s => 
            s.EndTime != null && 
            s.Token.Direction.Id == directionId);
        
        return await query.Select(s => new SessionDto(
                s.Id,
                s.StartTime,
                s.EndTime,
                s.Score,
                new TokenShortDto(
                    s.Token.Id,
                    s.Token.DirectionId,
                    s.Token.Direction.Name,
                    s.Token.Status)))
            .ToListAsync();
    }

    public async Task<Session?> GetSessionAsync(Guid tokenId)
    {
        var sessions = await _context.Sessions.Where(s => s.TokenId == tokenId).ToListAsync();
        return sessions.SingleOrDefault();
    }

    public async Task<SessionDto?> GetActualSessionAsync(Guid tokenId)
    {
        var session =  await _context.Sessions
            .Include(session => session.Token)
            .ThenInclude(token => token.Direction)
            .SingleOrDefaultAsync(s => s.TokenId == tokenId && s.EndTime == null);
        return session is null 
            ? null 
            : new SessionDto(
                session.Id, 
                session.StartTime, 
                session.EndTime, 
                session.Score, 
                new TokenShortDto(
                    session.Token.Id, 
                    session.Token.Direction.Id, 
                    session.Token.Direction.Name, 
                    session.Token.Status));
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
    
    private async Task CreateSessionUserTopics(Session session)
    {
        var middleGrade = _context.Grades.Single(g => g.Code == "Middle");
        var weight = _context.Weights.Single(w => w.Grade == middleGrade);
        var topics = _context.Topics.Where(t => t.Direction == session.Token.Direction);
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