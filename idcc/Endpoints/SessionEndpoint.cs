using System.Security.Claims;
using idcc.Dtos;
using idcc.Infrastructures;
using idcc.Providers;
using idcc.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace idcc.Endpoints;

[ApiController]
[Route("api/session")]
[Authorize(Roles = "Employee,Person")]
public class SessionController : ControllerBase
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IDurationProvider _durationProvider;
    public SessionController(ISessionRepository sessionRepository, IDurationProvider durationProvider)
    {
        _sessionRepository = sessionRepository;
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
        if (dto.TokenId == Guid.Empty)
        {
            return BadRequest(new ResponseDto("TokenId is required"));
        }
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var role   = User.IsInRole("Employee") ? "Employee" : "Person";
        var isEmployee = role == "Employee";
            
        var session = await _sessionRepository.StartSessionAsync(userId, isEmployee, dto.TokenId);
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
    /// <summary>Досрочно (или штатно) завершает сессию тестирования.</summary>
    /// <remarks>
    /// <para>
    /// • <paramref name="sessionId"/> — идентификатор сессии.<br/>
    /// • <paramref name="faster"/> = <see langword="true"/> означает принудительное
    /// завершение, даже если вопросы не исчерпаны.
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
    [ProducesResponseType(typeof(SessionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string),    StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StopSession(int sessionId, bool faster)
    {
        if (sessionId <= 0)
        {
            return BadRequest("sessionId must be > 0");
        }
        var session = await _sessionRepository.GetSessionAsync(sessionId);
        if (session is null)
        {
            return BadRequest(new ResponseDto(ErrorMessages.SESSION_IS_NOT_EXIST));
        }
            
        if (session.EndTime is not null)
        {
            return BadRequest(new ResponseDto(ErrorMessages.SESSION_IS_FINISHED));
        }
        var isFinished = await _sessionRepository.EndSessionAsync(sessionId, faster);
        return isFinished ? Ok() : BadRequest(new ResponseDto(ErrorMessages.SESSION_IS_NOT_FINISHED));
    }

    // ═══════════════════════════════════════════════════════
    // GET /sessions
    // ═══════════════════════════════════════════════════════
    /// <summary>История всех сессий физического лица.</summary>
    [HttpGet("sessions")]
    [ProducesResponseType(typeof(IEnumerable<SessionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Sessions(){
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var sessions = await _sessionRepository.GetSessionsForUserAsync(userId,false);
        return Ok(sessions);
    }
}