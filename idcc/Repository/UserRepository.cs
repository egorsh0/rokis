using idcc.Context;
using idcc.Models;
using idcc.Models.Profile;
using idcc.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace idcc.Repository;

public class UserRepository : IUserRepository
{
    private readonly IdccContext _context;

    public UserRepository(IdccContext context)
    {
        _context = context;
    }
    
    public async Task<UserProfile?> GetUserAsync(int id)
    {
        return await _context.UserProfiles.SingleOrDefaultAsync(u => u.Id == id);
    }

    public async Task<UserProfile?> GetUserByNameAsync(string name)
    {
        return await _context.UserProfiles.FindAsync(name);
    }

    public async Task<UserProfile> CreateAsync(UserProfile userProfile)
    {
        var entryUser = await _context.UserProfiles.AddAsync(userProfile);
        await _context.SaveChangesAsync();
        return entryUser.Entity;
    }
    
    public Task<Role?> GetRoleAsync(string code)
    {
        return _context.Roles.SingleOrDefaultAsync(r => r.Code == code);
    }
}