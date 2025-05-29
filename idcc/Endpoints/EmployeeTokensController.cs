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
    public EmployeeTokensController(ITokenRepository tokenRepository)
    {
        _tokenRepository = tokenRepository;
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
            return BadRequest(new ResponseDto("TokenId is required"));
        }
        var ok = await _tokenRepository.BindUsedTokenToPersonAsync(dto.TokenId, dto.UserEmail);
        return ok ? Ok() : BadRequest(new ResponseDto("Cannot bind used token"));
    }
}