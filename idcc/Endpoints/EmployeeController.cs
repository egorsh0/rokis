using System.Security.Claims;
using idcc.Dtos;
using idcc.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace idcc.Endpoints;

/// <summary>Методы, доступные сотруднику-владельцу токенов.</summary>
[ApiController]
[Route("api/employee")]
[Authorize(Roles = "Employee")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(IEmployeeRepository employeeRepository,
                             ILogger<EmployeeController> logger)
    {
        _employeeRepository = employeeRepository;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════
    //  GET /api/employee
    // ═══════════════════════════════════════════════════════
    /// <summary>Возвращает профиль сотрудника.</summary>
    /// <response code="200">Успешно, JSON с информацией о сотруднике.</response>
    /// <response code="404">Сотрудник не найден (маловероятно, если токен валиден).</response>
    [HttpGet]
    [ProducesResponseType(typeof(EmployeeProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string),                 StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmployee()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var emp    = await _employeeRepository.GetEmployeeWithCompanyAsync(userId);

        if (emp == null)
        {
            return NotFound("Employee not found");
        }

        var dto = new EmployeeProfileDto(
            emp.Id,
            emp.FullName,
            emp.Email,
            emp.Company is null
                ? null
                : new CompanyProfileShortDto(
                    emp.Company.Id,
                    emp.Company.FullName,
                    emp.Company.INN,
                    emp.Company.Email));

        return Ok(dto);
    }
}