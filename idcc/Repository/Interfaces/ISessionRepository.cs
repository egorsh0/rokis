using idcc.Dtos;
using idcc.Models;

namespace idcc.Repository.Interfaces;

public interface ISessionRepository
{
    Task<SessionResultDto> StartSessionAsync(string userId, bool isEmployee, Guid tokenId);

    Task<StopSessionDto> EndSessionAsync(Guid tokenId);
    
    Task<IEnumerable<SessionDto>> GetSessionsForUserAsync(string userId, bool isEmployee);

    Task<IEnumerable<SessionDto>> GetCloseSessionsAsync(int directionId);

    Task<Session?> GetSessionAsync(Guid tokenId);
    
    Task<Session?> GetActualSessionAsync(Guid tokenId);
    
    Task<Session?> GetFinishSessionAsync(Guid tokenId);
    
    Task SessionScoreAsync(int sessionId, double score);
}