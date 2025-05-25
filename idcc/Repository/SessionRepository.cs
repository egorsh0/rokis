using idcc.Context;
using idcc.Dtos;
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
        var token = await _context.Tokens.FindAsync(tokenId);
        if (token == null)
        {
            return new SessionResultDto(null, tokenId, false, "Token not found");
        }

        if (token.Status != TokenStatus.Bound)
        {
            return new SessionResultDto(null, tokenId, false, "Token not bound");
        }

        switch (isEmployee)
        {
            // Проверим, что текущий userId совпадает с тем, кому он привязан
            case true when token.EmployeeUserId != userId:
            case false when token.PersonUserId != userId:
                return new SessionResultDto(null, tokenId, false, "Forbidden");
        }

        var session = new Session {
            TokenId = tokenId,
            StartTime = DateTime.UtcNow,
            EmployeeUserId = isEmployee ? userId : null,
            PersonUserId = isEmployee ? null : userId
        };
        _context.Sessions.Add(session);
        await CreateSessionUserTopics(session);
        await _context.SaveChangesAsync();
        return new SessionResultDto(session.Id, session.TokenId, true, null);
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
    
    public async Task<IEnumerable<SessionDto>> GetSessionsForUserAsync(
        string userId,
        bool   isEmployee)
    {
        // базовый запрос
        IQueryable<Session> query = _context.Sessions
            .AsNoTracking()
            .Include(s => s.Token)                // 1-й Include
            .ThenInclude(t => t.Direction);   // 2-й Include

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

        session.Score        = score;
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