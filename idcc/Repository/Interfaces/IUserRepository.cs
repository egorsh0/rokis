using idcc.Models;

namespace idcc.Repository.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserAsync(int id);
    Task<User?> GetUserByNameAsync(string name);
    Task<User> CreateAsync(User user);
    Task<Role?> GetRoleAsync(string name);
}