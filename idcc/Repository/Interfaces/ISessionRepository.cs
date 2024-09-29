using idcc.Models;

namespace idcc.Repository.Interfaces;

public interface ISessionRepository
{
    Task<Session> StartSessionAsync(User user);
    
    Task<bool> EndSessionAsync(int id);

    Task<Session?> GetSessionAsync(int id);
}