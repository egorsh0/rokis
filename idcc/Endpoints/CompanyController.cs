using System.Security.Claims;
using idcc.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace idcc.Endpoints;

[ApiController]
[Route("api/company")]
[Authorize(Roles="Company")]
public class CompanyController : ControllerBase
{
    private readonly ICompanyRepository _companyRepository;
    private readonly ILogger<CompanyController> _logger;

    public CompanyController(ICompanyRepository companyRepository,
                             ILogger<CompanyController> logger)
    {
        _companyRepository = companyRepository;
        _logger = logger;
    }

    /// <summary>
    /// Привязать сотрудника (employeeEmail) к компании (companyUserId).
    /// Допустим, у нас уже есть авторизация, 
    /// и companyUserId можно взять из токена. 
    /// Но для примера передаём параметрами.
    /// </summary>
    [HttpPost("attach-employee")]
    public async Task<IActionResult> AttachEmployee([FromQuery] string employeeEmail)
    {
        var companyUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var result = await _companyRepository.AttachEmployeeToCompanyAsync(companyUserId, employeeEmail);
        if (!result)
        {
            return NotFound("Either company or employee not found");
        }

        _logger.LogInformation("Attached employee {Email} to company {CompanyUserId}", employeeEmail, companyUserId);
        return Ok("Employee attached successfully");
    }

    /// <summary>
    /// Получить компанию со списком сотрудников
    /// </summary>
    [HttpGet("/")]
    public async Task<IActionResult> GetCompany()
    {
        var companyUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var company = await _companyRepository.GetCompanyWithEmployeesAsync(companyUserId);
        if (company == null)
        {
            return NotFound("Company not found");
        }

        return Ok(new
        {
            company.Id,
            Name = company.FullName,
            company.INN,
            company.Email,
            Employees = company.Employees.Select(e => new
            {
                e.Id,
                e.FullName,
                e.Email
            })
        });
    }
}