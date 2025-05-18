using idcc.Context;
using idcc.Dtos;
using idcc.Models.Profile;
using idcc.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace idcc.Repository;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly IdccContext _idccContext;
    public EmployeeRepository(IdccContext idccContext) => _idccContext = idccContext;

    public async Task<EmployeeProfile?> GetEmployeeWithCompanyAsync(string employeeUserId) =>
        await _idccContext.EmployeeProfiles
            .Include(ep => ep.Company)
            .FirstOrDefaultAsync(ep => ep.UserId == employeeUserId);
    
    public async Task<bool> UpdateEmployeeAsync(string userId, UpdateEmployeeDto dto)
    {
        var emp = await _idccContext.EmployeeProfiles
            .FirstOrDefaultAsync(e => e.UserId == userId);
        if (emp is null)
            return false;

        bool changed = false;

        if (dto.FullName is not null && dto.FullName != emp.FullName)
        {
            emp.FullName = dto.FullName;
            changed = true;
        }

        if (dto.Email is not null && dto.Email != emp.Email)
        {
            emp.Email = dto.Email;
            changed = true;
        }

        if (!changed)
            return false;

        await _idccContext.SaveChangesAsync();
        return true;
    }
}