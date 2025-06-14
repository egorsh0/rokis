﻿using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using rokis.Dtos;
using rokis.Extensions;
using rokis.Infrastructures;
using rokis.Providers;
using rokis.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace rokis.Endpoints;

[ApiController]
[Route("session")]
[Authorize(Roles = "Employee,Person")]
public class SessionController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly IDurationProvider _durationProvider;
    public SessionController(
        ISessionService sessionService,
        IDurationProvider durationProvider)
    {
        _sessionService = sessionService;
        _durationProvider = durationProvider;
    }
    
    // ═══════════════════════════════════════════════════════
    // POST /session/start
    // ═══════════════════════════════════════════════════════
    /// <summary>Запуск сессии тестирования для токена.</summary>
    /// <response code="200">Сессия создана (SessionDto).</response>
    /// <response code="400">Токен не принадлежит пользователю / неверный статус.</response>
    [HttpPost("start")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(SessionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string),    StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StartSession([FromBody]StartSessionDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var role   = User.IsInRole("Employee") ? "Employee" : "Person";
        var isEmployee = role == "Employee";
            
        var session = await _sessionService.StartSessionAsync(userId, isEmployee, dto.TokenId);
        if (!session.Succeeded)
        {
            return BadRequest(session);
        }
        session.DurationTime = await _durationProvider.GetDurationAsync();
        return Ok(session);

    }
    
    // ═══════════════════════════════════════════════════════
    // POST /session/stop
    // ═══════════════════════════════════════════════════════
    /// <summary>Завершает сессию тестирования.</summary>
    /// <remarks>
    /// <para>
    /// • <paramref name="tokenId"/> — идентификатор токена для сессии.<br/>
    /// </para>
    /// </remarks>
    /// <response code="200">Сессия успешно завершена.</response>
    /// <response code="400">
    /// <list type="bullet">
    ///   <item><description><c>Сессии не существует.</c></description></item>
    ///   <item><description><c>Сессия завершена.</c></description></item>
    ///   <item><description><c>Сессия не завершена.</c></description></item>
    /// </list>
    /// </response>
    [HttpPost("stop")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string),    StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StopSession([Required] Guid tokenId)
    {
        var session = await _sessionService.GetSessionAsync(tokenId);
        if (session is null)
        {
            return BadRequest(new ResponseDto(MessageCode.SESSION_IS_NOT_EXIST, MessageCode.SESSION_IS_NOT_EXIST.GetDescription()));
        }
            
        if (session.EndTime is not null)
        {
            return BadRequest(new ResponseDto(MessageCode.SESSION_IS_FINISHED, MessageCode.SESSION_IS_FINISHED.GetDescription()));
        }
        var dto = await _sessionService.EndSessionAsync(tokenId);
        return dto.isSuccess ? Ok(new ResponseDto(dto.Code, dto.Message)) : BadRequest(new ResponseDto(dto.Code, dto.Message));
    }

    // ═══════════════════════════════════════════════════════
    // GET /sessions
    // ═══════════════════════════════════════════════════════
    /// <summary>История всех сессий физического лица.</summary>
    [HttpGet("sessions")]
    [ProducesResponseType(typeof(IEnumerable<SessionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Sessions(){
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var sessions = await _sessionService.GetUserSessionsAsync(userId,false);
        return Ok(sessions);
    }
}