using idcc.Models;
using idcc.Models.Profile;

namespace idcc.Repository.Interfaces;

public interface ISessionRepository
{
    Task<Session> StartSessionAsync(string userId, bool isEmployee, Guid tokenId);

    Task<bool> EndSessionAsync(int id, bool faster);
    
    Task<IEnumerable<Session>> GetSessionsForUserAsync(string userId, bool isEmployee);

    Task<Session?> GetSessionAsync(int id);
    
    Task<Session?> GetActualSessionAsync(Guid tokenId);
    
    Task<Session?> GetFinishSessionAsync(Guid tokenId);
    
    Task SessionScoreAsync(int id, double score);
}