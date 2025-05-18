using idcc.Dtos;
using idcc.Models.Profile;

namespace idcc.Repository.Interfaces;

public interface ICompanyRepository
{
    Task<bool> AttachEmployeeToCompanyAsync(string companyUserId, string employeeEmail);
    Task<CompanyProfile?> GetCompanyWithEmployeesAsync(string companyUserId);
    Task<bool> UpdateCompanyAsync(string userId, UpdateCompanyDto dto);
}