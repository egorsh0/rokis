using idcc.Dtos;
using idcc.Repository.Interfaces;

namespace idcc.Service;

public interface ISessionService
{
    Task<SessionDto?> GetFinishSessionAsync(Guid tokenId);
    Task SessionScoreAsync(int sessionId, double score);
    Task<SessionDto?> GetSessionAsync(Guid tokenId);
}

public class SessionService : ISessionService
{
    private readonly ISessionRepository _sessionRepository;

    public SessionService(ISessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
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