using idcc.Models;

namespace idcc.Repository.Interfaces;

public interface ISessionRepository
{
    Task StartSessionAsync(User user);
    
    Task EndSessionAsync(User user);

    Task<Session?> GetSessionAsync(int userId);
}