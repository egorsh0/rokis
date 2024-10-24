using idcc.Models;

namespace idcc.Repository.Interfaces;

public interface ISessionRepository
{
    Task<Session> StartSessionAsync(User user, Role role);

    Task<bool> EndSessionAsync(int id, bool faster);

    Task<Session?> GetSessionAsync(int id);

    Task<List<Session>> GetSessionsAsync(User user);
    
    Task<Session?> GetActualSessionAsync(string name);
    
    Task<Session?> GetFinishSessionAsync(string name);
    
    Task SessionScoreAsync(int id, double score);
}