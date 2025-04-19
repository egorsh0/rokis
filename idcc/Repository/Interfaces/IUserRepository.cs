using idcc.Models;
using idcc.Models.Profile;

namespace idcc.Repository.Interfaces;

public interface IUserRepository
{
    Task<PersonProfile?> GetUserAsync(int id);
    Task<PersonProfile?> GetUserByNameAsync(string name);
    Task<PersonProfile> CreateAsync(PersonProfile employee);
    Task<Direction?> GetRoleAsync(string name);
}