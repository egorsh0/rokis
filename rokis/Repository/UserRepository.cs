using rokis.Context;
using rokis.Models;
using rokis.Models.Profile;
using Microsoft.EntityFrameworkCore;

namespace rokis.Repository;

public interface IUserRepository
{
    Task<PersonProfile?> GetUserAsync(int id);
    Task<PersonProfile?> GetUserByNameAsync(string name);
    Task<PersonProfile> CreateAsync(PersonProfile employee);
    Task<Direction?> GetRoleAsync(string name);
}

public class UserRepository : IUserRepository
{
    private readonly RokisContext _context;

    public UserRepository(RokisContext context)
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