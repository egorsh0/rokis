using System.Security.Claims;
using idcc.Dtos;
using idcc.Extensions;
using idcc.Infrastructures;
using idcc.Models;
using idcc.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace idcc.Endpoints;

/// <summary>Методы, доступные сотруднику-владельцу токенов.</summary>
[ApiController]
[Route("api/employee")]
[Authorize(Roles = "Employee")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(IEmployeeRepository employeeRepository,
        UserManager<ApplicationUser> userManager,
                             ILogger<EmployeeController> logger)
    {
        _employeeRepository = employeeRepository;
        _userManager = userManager;
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
            return NotFound(new ResponseDto(MessageCode.EMPLOYEE_NOT_FOUND, MessageCode.EMPLOYEE_NOT_FOUND.GetDescription()));
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
                    emp.Company.LegalAddress,
                    emp.Company.INN,
                    emp.Company.Kpp,
                    emp.Company.Email));

        return Ok(dto);
    }
    
    // PATCH /api/employee   (частичное обновление)
    /// <summary>
    /// Частичное обновление данных сотрудника
    /// </summary>
    /// <param name="dto">Модель сотрудника для частичного обновления</param>
    /// <response code="200">Успешно - данные изменены (The data is changed).</response>
    /// <response code="400">Данные не обновлены.</response>
    [HttpPatch]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [HttpPatch]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Patch([FromBody] UpdateEmployeeDto dto)
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var updateResult  = await _employeeRepository.UpdateEmployeeAsync(uid, dto);
        return updateResult.Succeeded ? Ok(new ResponseDto(MessageCode.UPDATE_IS_FINISHED,MessageCode.UPDATE_IS_FINISHED.GetDescription())) : BadRequest(new ResponseDto(MessageCode.UPDATE_HAS_ERRORS,string.Join(Environment.NewLine, updateResult.Errors)));
    }
    
    // POST /api/employee/change-password
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userManager.GetUserAsync(User);
        var res  = await _userManager.ChangePasswordAsync(user!, dto.OldPassword, dto.NewPassword);
        return res.Succeeded ? NoContent() : BadRequest(res.Errors.Select(e => e.Description));
    }
}