using idcc.Models;
using idcc.Models.Profile;

namespace idcc.Repository.Interfaces;

public interface IUserRepository
{
    Task<UserProfile?> GetUserAsync(int id);
    Task<UserProfile?> GetUserByNameAsync(string name);
    Task<UserProfile> CreateAsync(UserProfile employee);
    Task<Role?> GetRoleAsync(string name);
}