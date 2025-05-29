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
    public PersonTokensController(ITokenRepository tokenRepository)
    {
        _tokenRepository = tokenRepository;
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
}