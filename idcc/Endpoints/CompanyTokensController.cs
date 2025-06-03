using System.Security.Claims;
using idcc.Dtos;
using idcc.Extensions;
using idcc.Infrastructures;
using idcc.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace idcc.Endpoints;

/// <summary>Действия компании с токенами: покупка, список, привязка к сотруднику.</summary>
[ApiController]
[Route("api/company/tokens")]
[Authorize(Roles = "Company")]
public class CompanyTokensController : ControllerBase
{
    private readonly ITokenRepository _tokenRepository;
    public CompanyTokensController(ITokenRepository tokenRepository) => _tokenRepository = tokenRepository;

    // ═══════════════════════════════════════════════════
    //  GET /api/company/tokens
    // ═══════════════════════════════════════════════════
    /// <summary>Возвращает все токены, купленные компанией.</summary>
    /// <response code="200">Список токенов.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TokenDto>), StatusCodes.Status200OK)]

    public async Task<IActionResult> List()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var tokens = await _tokenRepository.GetTokensForCompanyAsync(userId);
        return Ok(tokens);
    }

    // ═══════════════════════════════════════════════════
    //  POST /bind
    // ═══════════════════════════════════════════════════
    /// <summary>Привязывает неиспользованный токен к сотруднику.</summary>
    /// <remarks>
    /// <para>
    /// Токен должен быть в статусе <c>Unused</c>, сотрудник — существовать и
    /// принадлежать текущей компании. После привязки статус токена становится <c>Bound</c>.
    /// </para>
    /// </remarks>
    /// <param name="dto">Id токена и email сотрудника.</param>
    /// <response code="200">Успешно привязано.</response>
    /// <response code="400">Токен не найден / сотрудник чужой / уже привязан.</response>
    [HttpPost("bind")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Bind([FromBody] BindTokenDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var code = await _tokenRepository.BindTokenToEmployeeAsync(dto.TokenId, dto.EmployeeEmail, userId);
        if (code != MessageCode.BIND_IS_FINISHED)
        {
            return BadRequest(new ResponseDto(code, code.GetDescription()));
        }
        return Ok(new ResponseDto(code, code.GetDescription()));
    }
}