using System.Security.Claims;
using idcc.Dtos;
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
    //  POST /purchase
    // ═══════════════════════════════════════════════════
    /// <summary>Покупает партию токенов.</summary>
    /// <remarks>
    /// <b>Сценарий:</b> компания выбирает направление и количество токенов,
    /// В ответ приходит созданный заказ.
    /// </remarks>
    /// <response code="200">Объект заказа (Id, Qty, Prices, Tokens[]).</response>
    /// <response code="400">Ошибочный запрос (пустой список и т.п.).</response>
    [HttpPost("purchase")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string),   StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Purchase([FromBody]CreateOrderDto dto)
    {
        if (dto.Items.Count == 0)
        {
            return BadRequest("Items array must not be empty");
        }
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var order = await _tokenRepository.PurchaseAsync(userId, "Company", dto.Items);
        return Ok(order);
    }

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
        var ok = await _tokenRepository.BindTokenToEmployeeAsync(dto.TokenId, dto.EmployeeEmail, userId);
        if (!ok)
        {
            return BadRequest("Cannot bind (token or employee invalid)");
        }
        return Ok();
    }
}