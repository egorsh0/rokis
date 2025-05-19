using System.Security.Claims;
using idcc.Dtos;
using idcc.Models;
using idcc.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace idcc.Endpoints;

/// <summary>Методы, доступные физ лицу-владельцу токенов.</summary>
[ApiController]
[Route("api/person")]
[Authorize(Roles = "Person")]
public class PersonController : ControllerBase
{
    private readonly IPersonRepository _personRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<PersonController> _logger;

    public PersonController(IPersonRepository personRepository,
        UserManager<ApplicationUser> userManager,
        ILogger<PersonController> logger)
    {
        _personRepository = personRepository;
        _userManager = userManager;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════
    //  GET /api/person
    // ═══════════════════════════════════════════════════════
    /// <summary>Возвращает профиль физ лица.</summary>
    /// <response code="200">Успешно, JSON с информацией о сотруднике.</response>
    /// <response code="404">Сотрудник не найден (маловероятно, если токен валиден).</response>
    [HttpGet]
    [ProducesResponseType(typeof(PersonProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPerson()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var person = await _personRepository.GetPersonAsync(userId);

        if (person == null)
        {
            return NotFound(new ResponseDto("Person not found"));
        }

        var dto = new PersonProfileDto(
            person.Id,
            person.FullName,
            person.Email);

        return Ok(dto);
    }
    
    // PATCH /api/person   (частичное обновление)
    [HttpPatch]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Patch([FromBody] UpdatePersonDto dto)
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var updateResult  = await _personRepository.UpdatePersonAsync(uid, dto);
        return updateResult.Succeeded ? NoContent() : BadRequest(new ResponseDto(updateResult.Errors));
    }
    
    // POST /api/person/change-password
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        if (dto.NewPassword != dto.ConfirmNewPassword)
        {
            return BadRequest(new ResponseDto("Passwords do not match"));
        }

        var user = await _userManager.GetUserAsync(User);
        var res  = await _userManager.ChangePasswordAsync(user!, dto.OldPassword, dto.NewPassword);
        return res.Succeeded ? NoContent() : BadRequest(res.Errors.Select(e => e.Description));
    }
}