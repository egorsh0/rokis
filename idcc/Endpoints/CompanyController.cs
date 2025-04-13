using idcc.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace idcc.Endpoints;

[ApiController]
[Route("api/[controller]")]
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
    public async Task<IActionResult> AttachEmployee([FromQuery] string companyUserId, [FromQuery] string employeeEmail)
    {
        // Могли бы userId взять из User.Claims, если компания авторизована
        // var companyUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(companyUserId) || string.IsNullOrEmpty(employeeEmail))
        {
            return BadRequest("companyUserId or employeeEmail is missing");
        }

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
    public async Task<IActionResult> GetCompany([FromQuery] string companyUserId)
    {
        // Опять же, реально бы взяли из токена, 
        // но для примера – из query param.
        var company = await _companyRepository.GetCompanyWithEmployeesAsync(companyUserId);
        if (company == null)
        {
            return NotFound("Company not found");
        }

        return Ok(new
        {
            Id = company.Id,
            Name = company.FullName,
            INN = company.INN,
            Email = company.Email,
            Employees = company.Employees.Select(e => new
            {
                e.Id,
                e.FullName,
                e.Email
            })
        });
    }
}