using idcc.Context;
using idcc.Helper;
using idcc.Models;
using idcc.Models.Dto;
using idcc.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace idcc.Repository;

public class CompanyRepository : ICompanyRepository
{
    private readonly IdccContext _context;

    public CompanyRepository(IdccContext context)
    {
        _context = context;
    }
    
    public async Task<Company> CreateAsync(RegisterCompanyDto payload)
    {
        var company = new Company
        {
            Name = payload.Name,
            Inn = payload.Inn,
            Email = payload.Email,
            PasswordHash = PasswordHelper.Hash(payload.Password)
        };
        var entryCompany = await _context.Companies.AddAsync(company);
        await _context.SaveChangesAsync();
        return entryCompany.Entity;
    }

    public async Task<CompanyDto?> GetAsync(SearchCompanyPayload payload)
    {
        var company = await _context.Companies.FindAsync(payload.Email, payload.Inn);
        return company == null ? null : new CompanyDto(company.Id, company.Name, company.Inn, company.Email);
    }

    public async Task<CompanyDto?> GetAsync(LoginCompanyPayload payload)
    {
        var company = await _context.Companies.Where(c =>
            (c.Email == payload.Email || c.Inn == payload.Inn) &&
            c.PasswordHash == PasswordHelper.Hash(payload.Password)).FirstOrDefaultAsync();
        return company == null ? null : new CompanyDto(company.Id, company.Name, company.Inn, company.Email);
    }
}