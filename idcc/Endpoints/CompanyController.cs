using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using idcc.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace idcc.Endpoints;

/// <summary>Методы, доступные компании-владельцу токенов.</summary>
[ApiController]
[Route("api/company")]
[Authorize(Roles = "Company")]
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

    // ═══════════════════════════════════════════════════════
    //  POST /api/company/attach-employee
    // ═══════════════════════════════════════════════════════
    /// <summary>Привязывает сотрудника к текущей компании.</summary>
    /// <remarks>
    /// <para>
    /// <b>Сценарий:</b> менеджер вводит email сотрудника,
    /// чтобы тот стал участником компании.
    /// </para>
    /// </remarks>
    /// <param name="employeeEmail">Email сотрудника, которого нужно привязать.</param>
    /// <response code="200">Сотрудник успешно привязан.</response>
    /// <response code="404">Компания или сотрудник не найдены.</response>
    /// <response code="400">Некорректный email.</response>
    [HttpPost("attach-employee")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AttachEmployee([FromQuery, EmailAddress] string employeeEmail)
    {
        var companyUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var result = await _companyRepository.AttachEmployeeToCompanyAsync(companyUserId!, employeeEmail);
        if (!result)
        {
            return NotFound("Either company or employee not found");
        }

        _logger.LogInformation("Attached employee {Email} to company {CompanyUserId}", employeeEmail, companyUserId);
        return Ok("Employee attached successfully");
    }

    // ═══════════════════════════════════════════════════════
    //  GET /api/company
    // ═══════════════════════════════════════════════════════
    /// <summary>Возвращает профиль компании и её сотрудников.</summary>
    /// <response code="200">Успешно, JSON с компанией и сотрудниками.</response>
    /// <response code="404">Компания не найдена (маловероятно, если токен валиден).</response>
    [HttpGet]
    [ProducesResponseType(typeof(CompanyWithEmployeesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string),                 StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCompany()
    {
        var companyUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var company = await _companyRepository.GetCompanyWithEmployeesAsync(companyUserId!);
        if (company == null)
        {
            return NotFound("Company not found");
        }

        var dto = new CompanyWithEmployeesDto(
            company.Id,
            company.FullName,
            company.INN,
            company.Email,
            company.Employees.Select(e => new EmployeeShortDto(e.Id, e.FullName, e.Email)));

        return Ok(dto);
    }
}

/// <summary>DTO для краткого описания сотрудника.</summary>
public record EmployeeShortDto(int Id, string FullName, string Email);

/// <summary>DTO «Компания + сотрудники».</summary>
public record CompanyWithEmployeesDto(
    int                       Id,
    string                    Name,
    string                    INN,
    string                    Email,
    IEnumerable<EmployeeShortDto> Employees);