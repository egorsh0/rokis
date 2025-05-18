using idcc.Context;
using idcc.Dtos;
using idcc.Models.Profile;
using idcc.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace idcc.Repository;

public class CompanyRepository : ICompanyRepository
{
    private readonly IdccContext _idccContext;

    public CompanyRepository(IdccContext idccContext)
    {
        _idccContext = idccContext;
    }

    /// <summary>
    /// Привязать существующего сотрудника по email к компании (companyUserId).
    /// </summary>
    public async Task<bool> AttachEmployeeToCompanyAsync(string companyUserId, string employeeEmail)
    {
        // 1. Ищем профиль компании
        var companyProfile = await _idccContext.CompanyProfiles
            .FirstOrDefaultAsync(cp => cp.UserId == companyUserId);
        if (companyProfile == null)
        {
            return false; // нет такой компании
        }

        // 2. Ищем профиль сотрудника по email
        var employeeProfile = await _idccContext.EmployeeProfiles
            .FirstOrDefaultAsync(ep => ep.Email == employeeEmail);
        if (employeeProfile == null)
        {
            return false; // нет такого сотрудника
        }

        // 3. Привязываем (устанавливаем CompanyProfileId)
        employeeProfile.CompanyProfileId = companyProfile.Id;

        _idccContext.EmployeeProfiles.Update(employeeProfile);
        await _idccContext.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Получить компанию (по ее UserId) вместе со списком сотрудников
    /// </summary>
    public async Task<CompanyProfile?> GetCompanyWithEmployeesAsync(string companyUserId)
    {
        // Загрузим CompanyProfile, вкл. список EmployeeProfiles
        var company = await _idccContext.CompanyProfiles
            .Include(cp => cp.Employees) // подгружаем список сотрудников
            .FirstOrDefaultAsync(cp => cp.UserId == companyUserId);

        return company;
    }
    
    public async Task<bool> UpdateCompanyAsync(string userId, UpdateCompanyDto dto)
    {
        var entity = await _idccContext.CompanyProfiles
            .FirstOrDefaultAsync(cp => cp.UserId == userId);
        if (entity is null)
            return false;

        var changed = false;

        if (dto.Name is not null && dto.Name != entity.FullName)
        {
            entity.FullName = dto.Name;
            changed = true;
        }

        if (dto.LegalAddress is not null && dto.LegalAddress != entity.LegalAddress)
        {
            entity.LegalAddress = dto.LegalAddress;
            changed = true;
        }

        if (dto.Email is not null && dto.Email != entity.Email)
        {
            entity.Email = dto.Email;
            changed = true;
        }

        if (dto.Inn is not null && dto.Inn != entity.INN)
        {
            entity.INN = dto.Inn;
            changed = true;
        }

        if (dto.Kpp is not null && dto.Kpp != entity.Kpp)
        {
            entity.Kpp = dto.Kpp;
            changed = true;
        }

        if (!changed)
            return false;

        await _idccContext.SaveChangesAsync();
        return true;
    }
}