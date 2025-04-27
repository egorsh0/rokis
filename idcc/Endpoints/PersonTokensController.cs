using System.Security.Claims;
using idcc.Dtos;
using idcc.Repository;
using idcc.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace idcc.Endpoints;

[ApiController]
[Route("api/person/tokens")]
[Authorize(Roles="Person")]
public class PersonTokensController : ControllerBase
{
    private readonly ITokenRepository _tokenRepository;
    private readonly ISessionRepository _sessionRepository;
    public PersonTokensController(ITokenRepository tokenRepository, SessionRepository sessionRepository)
    {
        _tokenRepository = tokenRepository;
        _sessionRepository = sessionRepository;
    }

    [HttpPost("purchase")]
    public async Task<IActionResult> Purchase([FromBody]CreateOrderDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var order = await _tokenRepository.PurchaseAsync(userId, "Person", dto.Items);
        return Ok(order);
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var tokens = await _tokenRepository.GetTokensForPersonAsync(userId);
        return Ok(tokens);
    }

    [HttpPost("bind-used")]
    public async Task<IActionResult> BindUsed([FromBody]BindUsedTokenDto dto)
    {
        var ok = await _tokenRepository.BindUsedTokenToPersonAsync(dto.TokenId,dto.UserEmail);
        return ok ? Ok() : BadRequest("Cannot bind used token");
    }

    [HttpPost("session/start")]
    public async Task<IActionResult> StartSession([FromBody]StartSessionDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var session = await _sessionRepository.StartSessionAsync(userId,false, dto.TokenId);
        if (session.Succeeded)
        {
            return Ok(session);
        }

        return BadRequest(session);
    }

    [HttpGet("sessions")]
    public async Task<IActionResult> Sessions(){
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var sessions = await _sessionRepository.GetSessionsForUserAsync(userId,false);
        return Ok(sessions);
    }
}