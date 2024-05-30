using idcc.Context;
using idcc.Infrastructures;
using idcc.Models;
using idcc.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Grade = idcc.Infrastructures.Grade;

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

    public async Task<User?> GetUserByNameAsync(string name)
    {
        return await _context.Users.SingleOrDefaultAsync(_ => _.FullName == name);
    }

    public async Task<User> CreateAsync(User user)
    {
        var entryUser = await _context.Users.AddAsync(user);
        var entryGrade = _context.Grades.Single(_ => _.Name == Grade.Junior.ToString());
        
        var topics = _context.Topics.Where(_ => _.Role == user.Role);

        foreach (var topic in topics)
        {
            _context.UserGrades.Add(new UserGrade()
            {
                User = entryUser.Entity,
                Current = entryGrade,
                Score = Settings.MinimalScore,
                Topic = topic
            });
        }

        await _context.SaveChangesAsync();
        return entryUser.Entity;
    }
}