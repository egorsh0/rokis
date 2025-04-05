using idcc.Models;

namespace idcc.Repository.Interfaces;

public interface IEmployeeRepository
{
    Task<Employee?> GetUserAsync(int id);
    Task<Employee?> GetEmployeeByNameAsync(string name);
    Task<Employee> CreateAsync(Employee employee);
    Task<Role?> GetRoleAsync(string name);
}