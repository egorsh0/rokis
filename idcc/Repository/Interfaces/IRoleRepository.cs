using idcc.Models;

namespace idcc.Repository.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetRoleAsync(string name);
}