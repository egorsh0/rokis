using idcc.Models;
using idcc.Models.Dto;

namespace idcc.Repository.Interfaces;

public interface ICompanyRepository
{
    public Task<Company> CreateAsync(RegisterCompanyDto company);
    
    public Task<CompanyDto?> GetAsync(SearchCompanyPayload payload);
    
    public Task<CompanyDto?> GetAsync(LoginCompanyPayload payload);
}