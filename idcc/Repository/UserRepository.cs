using System.Text.Json;
using idcc.Context;
using idcc.Models;
using idcc.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace idcc.Repository;

public class UserRepository : IUserRepository
{
    private readonly IdccContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(IdccContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
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
        var middleGrade = _context.Grades.Single(_ => _.Code == "Middle");
        var weight = _context.Weights.Single(_ => _.Grade == middleGrade);
        var topics = _context.Topics.Where(_ => _.Role == user.Role);

        foreach (var topic in topics)
        {
            var userTopic = new UserTopic()
            {
                User = entryUser.Entity,
                Topic = topic,
                Weight = weight.Min,
                Grade = middleGrade,
                IsFinished = false,
                WasPrevious = false,
                Actual = false
            };
            _logger.LogInformation($"Create user topic {JsonSerializer.Serialize(userTopic)}");
            _context.UserTopics.Add(userTopic);
        }

        await _context.SaveChangesAsync();
        return entryUser.Entity;
    }
    
    public Task<Role?> GetRoleAsync(string code)
    {
        return _context.Roles.SingleOrDefaultAsync(_ => _.Code == code);
    }
}