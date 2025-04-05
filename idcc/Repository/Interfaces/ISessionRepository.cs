using idcc.Models;

namespace idcc.Repository.Interfaces;

public interface ISessionRepository
{
    Task<Session> StartSessionAsync(Employee employee, Role role);

    Task<bool> EndSessionAsync(int id, bool faster);

    Task<Session?> GetSessionAsync(int id);

    Task<List<Session>> GetSessionsAsync(Employee employee);
    
    Task<Session?> GetActualSessionAsync(string name);
    
    Task<Session?> GetFinishSessionAsync(string name);
    
    Task SessionScoreAsync(int id, double score);
}