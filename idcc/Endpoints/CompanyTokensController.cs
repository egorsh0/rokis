using System.Security.Claims;
using idcc.Dtos;
using idcc.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace idcc.Endpoints;

[ApiController]
[Route("api/company/tokens")]
[Authorize(Roles="Company")]
public class CompanyTokensController : ControllerBase
{
    private readonly ITokenRepository _tokenRepository;
    public CompanyTokensController(ITokenRepository tokenRepository) => _tokenRepository=tokenRepository;

    [HttpPost("purchase")]
    public async Task<IActionResult> Purchase([FromBody]CreateOrderDto dto){
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var order = await _tokenRepository.PurchaseAsync(userId, "Company", dto.Items);
        return Ok(order);
    }

    [HttpGet]
    public async Task<IActionResult> List(){
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var tokens = await _tokenRepository.GetTokensForCompanyAsync(userId);
        return Ok(tokens);
    }

    [HttpPost("bind")]
    public async Task<IActionResult> Bind([FromBody] BindTokenDto dto){
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var ok = await _tokenRepository.BindTokenToEmployeeAsync(dto.TokenId,dto.EmployeeEmail,userId);
        if(!ok) return BadRequest("Cannot bind");
        return Ok();
    }
}