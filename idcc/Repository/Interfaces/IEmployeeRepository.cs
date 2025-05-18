using idcc.Dtos;
using idcc.Models.Profile;

namespace idcc.Repository.Interfaces;

public interface IEmployeeRepository
{
    Task<EmployeeProfile?> GetEmployeeWithCompanyAsync(string employeeUserId);
    Task<bool> UpdateEmployeeAsync(string userId, UpdateEmployeeDto dto);
}