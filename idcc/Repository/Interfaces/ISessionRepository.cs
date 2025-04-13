using idcc.Models;
using idcc.Models.Profile;

namespace idcc.Repository.Interfaces;

public interface ISessionRepository
{
    Task<Session> StartSessionAsync(UserProfile userProfile, Role role);

    Task<bool> EndSessionAsync(int id, bool faster);

    Task<Session?> GetSessionAsync(int id);

    Task<List<Session>> GetSessionsAsync(UserProfile userProfile);
    
    Task<Session?> GetActualSessionAsync(string name);
    
    Task<Session?> GetFinishSessionAsync(string name);
    
    Task SessionScoreAsync(int id, double score);
}