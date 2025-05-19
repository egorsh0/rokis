using System.Security.Claims;
using idcc.Dtos;
using idcc.Repository;
using idcc.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace idcc.Endpoints;

/// <summary>Действия физического лица с токенами.</summary>
[ApiController]
[Route("api/person/tokens")]
[Authorize(Roles = "Person")]
public class PersonTokensController : ControllerBase
{
    private readonly ITokenRepository _tokenRepository;
    private readonly ISessionRepository _sessionRepository;
    public PersonTokensController(ITokenRepository tokenRepository, SessionRepository sessionRepository)
    {
        _tokenRepository = tokenRepository;
        _sessionRepository = sessionRepository;
    }

    // ═══════════════════════════════════════════════════════
    // POST /purchase
    // ═══════════════════════════════════════════════════════
    /// <summary>Покупка токенов физическим лицом.</summary>
    /// <remarks>
    /// <b>Сценарий:</b> пользователь выбирает направление + количество.
    /// </remarks>
    /// <response code="200">Созданный заказ (OrderDto).</response>
    /// <response code="400">Пустой список позиций.</response>
    [HttpPost("purchase")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string),   StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Purchase([FromBody]CreateOrderDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var order = await _tokenRepository.PurchaseAsync(userId, "Person", dto.Items);
        return Ok(order);
    }

    // ═══════════════════════════════════════════════════════
    // GET /api/person/tokens
    // ═══════════════════════════════════════════════════════
    /// <summary>Список токенов, принадлежащих физическому лицу.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TokenDto>), StatusCodes.Status200OK)]

    public async Task<IActionResult> List()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var tokens = await _tokenRepository.GetTokensForPersonAsync(userId);
        return Ok(tokens);
    }

    // ═══════════════════════════════════════════════════════
    // POST /bind-used
    // ═══════════════════════════════════════════════════════
    /// <summary>Ретро-привязка уже использованного токена к себе.</summary>
    /// <remarks>
    /// Токен должен иметь статус <c>Used</c> и быть пройден именно этим email.
    /// </remarks>
    /// <response code="200">Токен привязан.</response>
    /// <response code="400">Нельзя привязать (токен или email не совпадают).</response>
    [HttpPost("bind-used")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BindUsed([FromBody]BindUsedTokenDto dto)
    {
        if (dto.TokenId == Guid.Empty)
        {
            return BadRequest(new ResponseDto("TokenId is required"));
        }
        var ok = await _tokenRepository.BindUsedTokenToPersonAsync(dto.TokenId,dto.UserEmail);
        return ok ? Ok() : BadRequest(new ResponseDto("Cannot bind used token"));
    }

    // ═══════════════════════════════════════════════════════
    // POST /session/start
    // ═══════════════════════════════════════════════════════
    /// <summary>Запуск сессии тестирования для токена.</summary>
    /// <response code="200">Сессия создана (SessionDto).</response>
    /// <response code="400">Токен не принадлежит пользователю / неверный статус.</response>
    [HttpPost("session/start")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(SessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string),    StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StartSession([FromBody]StartSessionDto dto)
    {
        if (dto.TokenId == Guid.Empty)
        {
            return BadRequest(new ResponseDto("TokenId is required"));
        }
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var session = await _sessionRepository.StartSessionAsync(userId,false, dto.TokenId);
        if (session.Succeeded)
        {
            return Ok(session);
        }

        return BadRequest(session);
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