using idcc.Context;
using idcc.Models;
using idcc.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace idcc.Repository;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly IdccContext _context;

    public EmployeeRepository(IdccContext context)
    {
        _context = context;
    }
    
    public async Task<Employee?> GetUserAsync(int id)
    {
        return await _context.Employees.SingleOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Employee?> GetEmployeeByNameAsync(string name)
    {
        return await _context.Employees.SingleOrDefaultAsync(u => u.Name == name);
    }

    public async Task<Employee> CreateAsync(Employee employee)
    {
        var entryUser = await _context.Employees.AddAsync(employee);
        await _context.SaveChangesAsync();
        return entryUser.Entity;
    }
    
    public Task<Role?> GetRoleAsync(string code)
    {
        return _context.Roles.SingleOrDefaultAsync(r => r.Code == code);
    }
}