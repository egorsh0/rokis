using System.Security.Claims;
using idcc.Dtos;
using idcc.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace idcc.Endpoints;

/// <summary>Действия сотрудника с токенами компании.</summary>
[ApiController]
[Route("api/employee/tokens")]
[Authorize(Roles = "Employee")]
public class EmployeeTokensController : ControllerBase
{
    private readonly ITokenRepository _tokenRepository;
    private readonly ISessionRepository _sessionRepository;
    public EmployeeTokensController(ITokenRepository tokenRepository, ISessionRepository sessionRepository)
    {
        _tokenRepository = tokenRepository;
        _sessionRepository = sessionRepository;
    }

    // ═══════════════════════════════════════════════════════
    // GET /api/employee/tokens
    // ═══════════════════════════════════════════════════════
    /// <summary>Список токенов, привязанных к текущему сотруднику.</summary>
    /// <response code="200">Массив токенов.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TokenDto>), StatusCodes.Status200OK)]

    public async Task<IActionResult> List(){
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var tokens = await _tokenRepository.GetTokensForEmployeeAsync(userId);
        return Ok(tokens);
    }

    // ═══════════════════════════════════════════════════════
    // POST /bind-used
    // ═══════════════════════════════════════════════════════
    /// <summary>Привязывает <с>использованный</с> токен к своему профилю.</summary>
    /// <remarks>
    /// Токен должен быть в статусе <c>Used</c> и уже содержать <c>Email</c>
    /// текущего сотрудника, иначе возвращается <c>400 BadRequest</c>.
    /// </remarks>
    /// <response code="200">Успешно привязан.</response>
    /// <response code="400">Нельзя привязать (токен или e-mail не совпадают).</response>
    [HttpPost("bind-used")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BindUsed([FromBody]BindUsedTokenDto dto)
    {
        if (dto.TokenId == Guid.Empty)
        {
            return BadRequest("TokenId is required");
        }
        var ok = await _tokenRepository.BindUsedTokenToPersonAsync(dto.TokenId, dto.UserEmail);
        return ok ? Ok() : BadRequest("Cannot bind used token");
    }

    // ═══════════════════════════════════════════════════════
    // POST /session/start
    // ═══════════════════════════════════════════════════════
    /// <summary>Запускает сессию тестирования для <c>Bound</c>-токена.</summary>
    /// <response code="200">Сессия создана (Id, TokenId, StartTime).</response>
    /// <response code="400">Токен не принадлежит сотруднику / неверный статус.</response>
    [HttpPost("session/start")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(SessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string),    StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StartSession([FromBody]StartSessionDto dto)
    {
        if (dto.TokenId == Guid.Empty)
        {
            return BadRequest("TokenId is required");
        }
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var session = await _sessionRepository.StartSessionAsync(userId, true, dto.TokenId);
        if (session.Succeeded)
        {
            return Ok(session);
        }

        return BadRequest(session);
    }

    // ═══════════════════════════════════════════════════════
    // GET /sessions
    // ═══════════════════════════════════════════════════════
    /// <summary>История всех сессий сотрудника.</summary>
    /// <response code="200">Массив сессий.</response>
    [HttpGet("sessions")]
    [ProducesResponseType(typeof(IEnumerable<SessionDto>), StatusCodes.Status200OK)]

    public async Task<IActionResult> Sessions()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var sessions = await _sessionRepository.GetSessionsForUserAsync(userId, true);
        return Ok(sessions);
    }
}