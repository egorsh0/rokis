using idcc.Context;
using idcc.Dtos;
using idcc.Models;
using idcc.Models.Profile;
using idcc.Repository.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace idcc.Repository;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly IdccContext _idccContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public EmployeeRepository(IdccContext idccContext, UserManager<ApplicationUser> userManager)
    {
        _idccContext = idccContext;
        _userManager = userManager;
    }

    public async Task<EmployeeProfile?> GetEmployeeWithCompanyAsync(string employeeUserId) =>
        await _idccContext.EmployeeProfiles
            .Include(ep => ep.Company)
            .FirstOrDefaultAsync(ep => ep.UserId == employeeUserId);
    
    public async Task<UpdateResult> UpdateEmployeeAsync(string userId, UpdateEmployeeDto dto)
    {
        var errors = new List<string>();
        
        // ── 1.  Профиль сотрудника ─────────────────────────────
        var profile = await _idccContext.EmployeeProfiles
            .FirstOrDefaultAsync(e => e.UserId == userId);

        if (profile is null)
        {
            return new UpdateResult(false, ["EMPLOYEE_NOT_FOUND"]);
        }
        
        // ── 2.  Пользователь Identity ──────────────────────────
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return new UpdateResult(false, ["EMPLOYEE_NOT_FOUND"]);
        }
        
        // ── 3.  Проверка email, если он меняется ───────────────
        if (!string.IsNullOrWhiteSpace(dto.Email) &&
            !string.Equals(dto.Email, profile.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _userManager.FindByEmailAsync(dto.Email);
            if (existing is not null && existing.Id != userId)
            {
                errors.Add("EMAIL_ALREADY_EXISTS");
            }
        }

        if (errors.Any())
        {
            return new UpdateResult(false, errors);
        }
        // ── 4.  Мапим not-null поля ────────────────────────────
        var changed = false;

        if (!string.IsNullOrWhiteSpace(dto.FullName) &&
            dto.FullName != profile.FullName)
        {
            profile.FullName = dto.FullName;
            changed = true;
        }

        if (!string.IsNullOrWhiteSpace(dto.Email) &&
            !string.Equals(dto.Email, profile.Email, StringComparison.OrdinalIgnoreCase))
        {
            // профиль
            profile.Email = dto.Email;

            // AspNetUsers
            user.Email = dto.Email;
            user.UserName = dto.Email;
            user.NormalizedEmail = _userManager.NormalizeEmail(dto.Email);
            user.NormalizedUserName = user.NormalizedEmail;

            var idRes = await _userManager.UpdateAsync(user);
            if (!idRes.Succeeded)
            {
                errors.AddRange(idRes.Errors.Select(e => e.Description));
                return new UpdateResult(false, errors);
            }
            changed = true;
        }

        if (!changed)
        {
            return new UpdateResult(false, ["NOTHING_TO_UPDATE"]);
        }

        await _idccContext.SaveChangesAsync();
        return new UpdateResult(true, new());
    }
}