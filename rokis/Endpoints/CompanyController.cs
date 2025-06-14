﻿using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using rokis.Dtos;
using rokis.Extensions;
using rokis.Infrastructures;
using rokis.Models;
using rokis.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace rokis.Endpoints;

/// <summary>Методы, доступные компании-владельцу токенов.</summary>
[ApiController]
[Route("company")]
[Authorize(Roles = "Company")]
public class CompanyController : ControllerBase
{
    private readonly ICompanyRepository _companyRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<CompanyController> _logger;

    public CompanyController(ICompanyRepository companyRepository,
        UserManager<ApplicationUser> userManager,
                             ILogger<CompanyController> logger)
    {
        _companyRepository = companyRepository;
        _userManager = userManager;
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
        if (!result.result)
        {
            return NotFound(new ResponseDto(result.code, result.code.GetDescription()));
        }

        _logger.LogInformation("Attached employee {Email} to company {CompanyUserId}", employeeEmail, companyUserId);
        return Ok(new ResponseDto(result.code, result.code.GetDescription()));
    }

    // ═══════════════════════════════════════════════════════
    //  GET /api/company
    // ═══════════════════════════════════════════════════════
    /// <summary>Возвращает профиль компании и её сотрудников.</summary>
    /// <response code="200">Успешно, JSON с компанией и сотрудниками.</response>
    /// <response code="404">Компания не найдена (маловероятно, если токен валиден).</response>
    [HttpGet]
    [ProducesResponseType(typeof(CompanyProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string),                 StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCompany()
    {
        var companyUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var company = await _companyRepository.GetCompanyWithEmployeesAsync(companyUserId!);
        if (company == null)
        {
            return NotFound(new ResponseDto(MessageCode.COMPANY_NOT_FOUND, MessageCode.COMPANY_NOT_FOUND.GetDescription()));
        }

        var dto = new CompanyProfileDto(
            company.Id,
            company.FullName,
            company.LegalAddress,
            company.INN,
            company.Kpp,
            company.Email,
            company.Employees.Select(e => new EmployeeProfileShortDto(e.Id, e.FullName, e.Email)));

        return Ok(dto);
    }
    
    // PATCH /api/company   (частичное обновление)
    /// <summary>
    /// Частичное обновление данных компании
    /// </summary>
    /// <param name="dto">Модель компании для частичного обновления</param>
    /// <response code="200">Успешно - данные изменены (The data is changed).</response>
    /// <response code="400">Данные не обновлены.</response>
    [HttpPatch]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PatchCompany([FromBody] UpdateCompanyDto dto)
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var updateResult  = await _companyRepository.UpdateCompanyAsync(uid, dto);
        return updateResult.Succeeded ? Ok(new ResponseDto(MessageCode.UPDATE_IS_FINISHED,MessageCode.UPDATE_IS_FINISHED.GetDescription())) : BadRequest(new ResponseDto(MessageCode.UPDATE_HAS_ERRORS,string.Join(Environment.NewLine, updateResult.Errors)));
    }
    
    // POST /api/company/change-password
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join(Environment.NewLine, ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)));
            return BadRequest(new ResponseDto(MessageCode.CHANGE_PASSWORD_FAILED, errors));
        }

        var user = await _userManager.GetUserAsync(User);
        var res  = await _userManager.ChangePasswordAsync(user!, dto.OldPassword, dto.NewPassword);

        return res.Succeeded
            ? NoContent()
            : BadRequest(new ResponseDto(MessageCode.PASSWORD_RESET_SUCCESSFUL, string.Join(Environment.NewLine, res.Errors.Select(e => e.Description))));
    }
}