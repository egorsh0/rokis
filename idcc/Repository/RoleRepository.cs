using idcc.Context;
using idcc.Models;
using idcc.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace idcc.Repository;

public class RoleRepository : IRoleRepository
{
    private readonly IdccContext _context;

    public RoleRepository(IdccContext context)
    {
        _context = context;
    }
    
    public Task<Role?> GetRoleAsync(string name)
    {
        return _context.Roles.SingleOrDefaultAsync(_ => _.Name == name);
    }
}