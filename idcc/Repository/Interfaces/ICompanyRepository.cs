using idcc.Dtos;
using idcc.Infrastructures;
using idcc.Models.Profile;

namespace idcc.Repository.Interfaces;

public interface ICompanyRepository
{
    Task<(bool result, MessageCode code)> AttachEmployeeToCompanyAsync(string companyUserId, string employeeEmail);
    Task<CompanyProfile?> GetCompanyWithEmployeesAsync(string companyUserId);
    Task<UpdateResult> UpdateCompanyAsync(string userId, UpdateCompanyDto dto);
}