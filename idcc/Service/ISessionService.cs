using idcc.Dtos;
using idcc.Extensions;
using idcc.Infrastructures;
using idcc.Repository;

namespace idcc.Service;

public interface ISessionService
{
    /// <summary>
    /// Начать сессию.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="isEmployee">Это сотрудник?</param>
    /// <param name="tokenId">Идентификатор токена.</param>
    /// <returns></returns>
    Task<SessionResultDto> StartSessionAsync(string userId, bool isEmployee, Guid tokenId);
    
    /// <summary>
    /// Остановить сессию.
    /// </summary>
    /// <param name="tokenId">Идентификатор токена.</param>
    /// <returns></returns>
    Task<StopSessionDto> EndSessionAsync(Guid tokenId);

    /// <summary>
    /// Получить актуальную сессию.
    /// </summary>
    /// <param name="tokenId">Идентификатор токена.</param>
    /// <returns></returns>
    Task<SessionDto?> GetActualSessionAsync(Guid tokenId);

    /// <summary>
    /// Получить пользовательские сессии.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="isEmployee">Это сотрудник?</param>
    /// <returns></returns>
    Task<List<SessionDto>> GetUserSessionsAsync(
        string userId,
        bool isEmployee);
    
    Task<SessionDto?> GetFinishSessionAsync(Guid tokenId);
    Task SessionScoreAsync(int sessionId, double score);
    Task<SessionDto?> GetSessionAsync(Guid tokenId);
}

public class SessionService : ISessionService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IUserTopicService _userTopicService;
    private readonly ITokenRepository _tokenRepository;

    public SessionService(
        ISessionRepository sessionRepository,
        IUserTopicService userTopicService,
        ITokenRepository tokenRepository)
    {
        _sessionRepository = sessionRepository;
        _userTopicService = userTopicService;
        _tokenRepository = tokenRepository;
    }

    public async Task<SessionResultDto> StartSessionAsync(string userId, bool isEmployee, Guid tokenId)
    {
        var token = await _tokenRepository.GetTokenAsync(tokenId);
        if (token is null || token.Status != TokenStatus.Bound)
        {
            return new SessionResultDto(null, tokenId, false, MessageCode.TOKEN_NOT_BOUND, MessageCode.TOKEN_NOT_BOUND.GetDescription());
        }
        
        switch (isEmployee)
        {
            // Проверим, что текущий userId совпадает с тем, кому он привязан
            case true when token.EmployeeUserId != userId:
            case false when token.PersonUserId != userId:
            {
                return new SessionResultDto(null, tokenId, false, MessageCode.TOKEN_IS_FORBIDDEN, MessageCode.TOKEN_IS_FORBIDDEN.GetDescription());
            }
        }
        
        var existing = await _sessionRepository.GetSessionAsync(tokenId, null);
        if (existing != null)
        {
            return new SessionResultDto(existing.Id, existing.TokenId, true, MessageCode.SESSION_HAS_ACTIVE,null);
        }

        var session = await _sessionRepository.CreateAsync(
            tokenId, 
            isEmployee ? userId : null, 
            !isEmployee ? userId : null);
        
        await _userTopicService.CreateUserTopicAsync(session.Id, session.Token.DirectionId);
        return new SessionResultDto(session.Id, session.TokenId, true, MessageCode.SESSION_IS_STARTED, null);

    }

    public async Task<StopSessionDto> EndSessionAsync(Guid tokenId)
    {
        var session = await _sessionRepository.GetSessionAsync(tokenId, null);
        if (session is null)
        {
            return new StopSessionDto(false, MessageCode.SESSION_IS_NOT_EXIST, MessageCode.SESSION_IS_NOT_EXIST.GetDescription());
        }

        var userTopics = await _userTopicService.GetUserTopicsAsync(session.Id);
        var openUserTopics = userTopics.Where(ut => ut.IsFinished == false).Select(ut => ut.Id).ToList();
        foreach (var userTopic in openUserTopics)
        {
            await _userTopicService.CloseUserTopicAsync(userTopic);
        }

        await _sessionRepository.CloseSessionAsync(session.Id);
        return new StopSessionDto(true, MessageCode.SESSION_IS_FINISHED, $"{tokenId} token session completed");
    }

    public async Task<SessionDto?> GetActualSessionAsync(Guid tokenId)
    {
        var session = await _sessionRepository.GetSessionAsync(tokenId, null);
        if (session is null)
        {
            return null;
        }
        
        return new SessionDto(
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

    public async Task<List<SessionDto>> GetUserSessionsAsync(string userId, bool isEmployee)
    {
        var sessions = await _sessionRepository.GetSessionsAsync(userId, isEmployee);
        // проекция в DTO (циклов больше нет)
        return sessions.Select(s => new SessionDto(
                s.Id,
                s.StartTime,
                s.EndTime,
                s.Score,
                new TokenShortDto(
                    s.Token.Id,
                    s.Token.DirectionId,
                    s.Token.Direction.Name,
                    s.Token.Status)))
            .ToList();
    }
    

    public async Task<SessionDto?> GetFinishSessionAsync(Guid tokenId)
    {
        var session = await _sessionRepository.GetFinishSessionAsync(tokenId);
        return session is null ? null : new SessionDto(session.Id, session.StartTime, session.EndTime, session.Score, new TokenShortDto(session.Token.Id, session.Token.Direction.Id, session.Token.Direction.Name, session.Token.Status));
    }

    public async Task SessionScoreAsync(int sessionId, double score)
    {
        await _sessionRepository.SessionScoreAsync(sessionId, score);
    }

    public async Task<SessionDto?> GetSessionAsync(Guid tokenId)
    {
        var session = await _sessionRepository.GetSessionAsync(tokenId);
        return session is null ? null : new SessionDto(session.Id, session.StartTime, session.EndTime, session.Score, new TokenShortDto(session.Token.Id, session.Token.Direction.Id, session.Token.Direction.Name, session.Token.Status));
    }
}