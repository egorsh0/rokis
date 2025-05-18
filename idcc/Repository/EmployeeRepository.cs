using idcc.Context;
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
}