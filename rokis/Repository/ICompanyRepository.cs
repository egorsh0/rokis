using rokis.Context;
using rokis.Dtos;
using rokis.Extensions;
using rokis.Infrastructures;
using rokis.Models;
using rokis.Models.Profile;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace rokis.Repository;

public interface ICompanyRepository
{
    Task<(bool result, MessageCode code)> AttachEmployeeToCompanyAsync(string companyUserId, string employeeEmail);
    Task<CompanyProfile?> GetCompanyWithEmployeesAsync(string companyUserId);
    Task<UpdateResult> UpdateCompanyAsync(string userId, UpdateCompanyDto dto);
}

public class CompanyRepository : ICompanyRepository
{
    private readonly RokisContext _rokisContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public CompanyRepository(RokisContext rokisContext, UserManager<ApplicationUser> userManager)
    {
        _rokisContext = rokisContext;
        _userManager = userManager;
    }

    /// <summary>
    /// Привязать существующего сотрудника по email к компании (companyUserId).
    /// </summary>
    public async Task<(bool, MessageCode)> AttachEmployeeToCompanyAsync(string companyUserId, string employeeEmail)
    {
        // 1. Ищем профиль компании
        var companyProfile = await _rokisContext.CompanyProfiles
            .FirstOrDefaultAsync(cp => cp.UserId == companyUserId);
        if (companyProfile == null)
        {
            return (false, MessageCode.COMPANY_NOT_FOUND);
        }

        // 2. Ищем профиль сотрудника по email
        var employeeProfile = await _rokisContext.EmployeeProfiles
            .FirstOrDefaultAsync(ep => ep.Email == employeeEmail);
        if (employeeProfile == null)
        {
            return (false, MessageCode.EMPLOYEE_NOT_FOUND);
        }

        // 3. Привязываем (устанавливаем CompanyProfileId)
        employeeProfile.CompanyProfileId = companyProfile.Id;

        _rokisContext.EmployeeProfiles.Update(employeeProfile);
        await _rokisContext.SaveChangesAsync();

        return (true, MessageCode.EMPLOYEE_ATTACHED);
    }

    /// <summary>
    /// Получить компанию (по ее UserId) вместе со списком сотрудников
    /// </summary>
    public async Task<CompanyProfile?> GetCompanyWithEmployeesAsync(string companyUserId)
    {
        // Загрузим CompanyProfile, вкл. список EmployeeProfiles
        var company = await _rokisContext.CompanyProfiles
            .Include(cp => cp.Employees) // подгружаем список сотрудников
            .FirstOrDefaultAsync(cp => cp.UserId == companyUserId);

        return company;
    }
    
    public async Task<UpdateResult> UpdateCompanyAsync(string userId, UpdateCompanyDto dto)
    {
        var errors = new List<(MessageCode code, string message)>();
        
        // 0. Загружаем профиль + пользователя один раз
        var profile = await _rokisContext.CompanyProfiles.FirstOrDefaultAsync(cp => cp.UserId == userId);
        if (profile is null)
        {
            return new UpdateResult(false, [(MessageCode.COMPANY_NOT_FOUND, MessageCode.COMPANY_NOT_FOUND.GetDescription())]);
        }
        
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return new UpdateResult(false, [(MessageCode.COMPANY_NOT_FOUND, MessageCode.COMPANY_NOT_FOUND.GetDescription())]);
        }
        
        // 1. Проверка Email (только если меняется)
        if (!string.IsNullOrWhiteSpace(dto.Email) &&
            !dto.Email.Equals(profile.Email, StringComparison.OrdinalIgnoreCase))
        {
            var exists = await _userManager.FindByEmailAsync(dto.Email);
            if (exists is not null && exists.Id != userId)
            {
                errors.Add((MessageCode.EMAIL_ALREADY_EXISTS, MessageCode.EMAIL_ALREADY_EXISTS.GetDescription()));
            }
        }

        // 2. Проверка ИНН (если меняется)
        if (!string.IsNullOrWhiteSpace(dto.Inn) && dto.Inn != profile.INN)
        {
            bool innBusy = await _rokisContext.CompanyProfiles
                .AnyAsync(cp => cp.INN == dto.Inn && cp.UserId != userId);

            if (innBusy)
            {
                errors.Add((MessageCode.INN_ALREADY_EXISTS, MessageCode.INN_ALREADY_EXISTS.GetDescription()));
            }
        }

        if (errors.Any())
        {
            return new UpdateResult(false, errors);
        }

        
        // 3. Мапим not-null поля
        bool changed = false;

        if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name != profile.FullName)
        {
            profile.FullName = dto.Name;
            changed = true;
        }

        if (!string.IsNullOrWhiteSpace(dto.LegalAddress) && dto.LegalAddress != profile.LegalAddress)
        {
            profile.LegalAddress = dto.LegalAddress;
            changed = true;
        }

        if (!string.IsNullOrWhiteSpace(dto.Kpp) && dto.Kpp != profile.Kpp)
        {
            profile.Kpp = dto.Kpp;
            changed = true;
        }

        if (!string.IsNullOrWhiteSpace(dto.Email) &&
            !dto.Email.Equals(profile.Email, StringComparison.OrdinalIgnoreCase))
        {
            // 3.1 профиль
            profile.Email = dto.Email;

            // 3.2 AspNetUsers
            user.Email = dto.Email;
            user.UserName = dto.Email;
            user.NormalizedEmail = _userManager.NormalizeEmail(dto.Email);
            user.NormalizedUserName = user.NormalizedEmail;

            var idRes = await _userManager.UpdateAsync(user);
            if (!idRes.Succeeded)
            {
                errors.Add((MessageCode.UPDATE_HAS_ERRORS ,string.Join(Environment.NewLine, idRes.Errors.Select(e => e.Description))));
                return new UpdateResult(false, errors);
            }
            changed = true;
        }

        if (!string.IsNullOrWhiteSpace(dto.Inn) && dto.Inn != profile.INN)
        {
            profile.INN = dto.Inn;
            changed = true;
        }

        if (!changed)
        {
            return new UpdateResult(false, [(MessageCode.NOTHING_TO_UPDATE, MessageCode.NOTHING_TO_UPDATE.GetDescription())]);
        }

        await _rokisContext.SaveChangesAsync();
        return new UpdateResult(true, []);
    }
}