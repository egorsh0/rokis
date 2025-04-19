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
    
    public async Task<PersonProfile?> GetUserAsync(int id)
    {
        return await _context.PersonProfiles.SingleOrDefaultAsync(u => u.Id == id);
    }

    public async Task<PersonProfile?> GetUserByNameAsync(string name)
    {
        return await _context.PersonProfiles.FindAsync(name);
    }

    public async Task<PersonProfile> CreateAsync(PersonProfile personProfile)
    {
        var entryUser = await _context.PersonProfiles.AddAsync(personProfile);
        await _context.SaveChangesAsync();
        return entryUser.Entity;
    }
    
    public Task<Direction?> GetRoleAsync(string code)
    {
        return _context.Directions.SingleOrDefaultAsync(r => r.Code == code);
    }
}