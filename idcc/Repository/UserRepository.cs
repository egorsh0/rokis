using idcc.Context;
using idcc.Models;
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
    
    public async Task<User?> GetUserAsync(int id)
    {
        return await _context.Users.SingleOrDefaultAsync(_ => _.Id == id);
    }

    public async Task<User?> GetUserByNameAsync(string username)
    {
        return await _context.Users.SingleOrDefaultAsync(_ => _.UserName == username);
    }

    public async Task<User> CreateAsync(User user)
    {
        var entryUser = await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return entryUser.Entity;
    }
    
    public Task<Role?> GetRoleAsync(string code)
    {
        return _context.Roles.SingleOrDefaultAsync(_ => _.Code == code);
    }
}