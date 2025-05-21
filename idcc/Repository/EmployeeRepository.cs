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
    
    public async Task<UpdateResult> UpdateEmployeeAsync(string userId, UpdateEmployeeDto dto)
    {
        var errors = new List<string>();
        
        var emp = await _idccContext.EmployeeProfiles
            .FirstOrDefaultAsync(e => e.UserId == userId);
        if (emp is null)
        {
            errors.Add("Employee not found");
            return new UpdateResult(false, errors);
        }
        
        // 1. Проверка Email (только если меняется)
        if (dto.Email is not null && dto.Email != emp.Email)
        {
            var emailBusy = await _idccContext.Users
                .AnyAsync(u => u.Email == dto.Email && u.Id != userId);

            if (emailBusy)
            {
                errors.Add("EMAIL_ALREADY_EXISTS");
            }
        }

        if (errors.Count > 0)
        {
            return new UpdateResult(false, errors);
        }
        
        var changed = false;

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
        {
            errors.Add("Could not update employee");
            return new UpdateResult(false, errors);
        }

        await _idccContext.SaveChangesAsync();
        return new UpdateResult(true, Array.Empty<string>().ToList());
    }
}