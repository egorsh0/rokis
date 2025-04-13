using idcc.Context;
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
}