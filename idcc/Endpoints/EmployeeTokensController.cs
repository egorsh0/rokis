using System.Security.Claims;
using idcc.Dtos;
using idcc.Repository;
using idcc.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace idcc.Endpoints;

[ApiController]
[Route("api/employee/tokens")]
[Authorize(Roles="Employee")]
public class EmployeeTokensController : ControllerBase
{
    private readonly ITokenRepository _tokenRepository;
    private readonly ISessionRepository _sessionRepository;
    public EmployeeTokensController(ITokenRepository tokenRepository, SessionRepository sessionRepository)
    {
        _tokenRepository = tokenRepository;
        _sessionRepository = sessionRepository;
    }

    [HttpGet]
    public async Task<IActionResult> List(){
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var tokens = await _tokenRepository.GetTokensForEmployeeAsync(userId);
        return Ok(tokens);
    }

    [HttpPost("bind-used")]
    public async Task<IActionResult> BindUsed([FromBody]BindUsedTokenDto dto){
        var ok = await _tokenRepository.BindUsedTokenToPersonAsync(dto.TokenId,dto.UserEmail);
        return ok ? Ok() : BadRequest("Cannot bind used token");
    }

    [HttpPost("session/start")]
    public async Task<IActionResult> StartSession([FromBody]StartSessionDto dto){
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var session = await _sessionRepository.StartSessionAsync(userId,true,dto.TokenId);
        return Ok(session);
    }

    [HttpGet("sessions")]
    public async Task<IActionResult> Sessions(){
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var sessions = await _sessionRepository.GetSessionsForUserAsync(userId,true);
        return Ok(sessions);
    }
}