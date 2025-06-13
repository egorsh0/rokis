using System.Security.Claims;
using idcc.Dtos;
using idcc.Extensions;
using idcc.Infrastructures;
using idcc.Models;
using idcc.Repository;
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
            return NotFound(new ResponseDto(MessageCode.PERSON_NOT_FOUND, MessageCode.PERSON_NOT_FOUND.GetDescription()));
        }

        var dto = new PersonProfileDto(
            person.Id,
            person.FullName,
            person.Email);

        return Ok(dto);
    }
    
    // PATCH /api/person   (частичное обновление)
    /// <summary>
    /// Частичное обновление данных клиента
    /// </summary>
    /// <param name="dto">Модель клиента для частичного обновления</param>
    /// <response code="200">Успешно - данные изменены (The data is changed).</response>
    /// <response code="400">Данные не обновлены.</response>
    [HttpPatch]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Patch([FromBody] UpdatePersonDto dto)
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var updateResult  = await _personRepository.UpdatePersonAsync(uid, dto);
        return updateResult.Succeeded ? Ok(new ResponseDto(MessageCode.UPDATE_IS_FINISHED,MessageCode.UPDATE_IS_FINISHED.GetDescription())) : BadRequest(new ResponseDto(MessageCode.UPDATE_HAS_ERRORS,string.Join(Environment.NewLine, updateResult.Errors)));
    }
    
    // POST /api/person/change-password
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join(Environment.NewLine, ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)));
            return BadRequest(new ResponseDto(MessageCode.CHANGE_PASSWORD_FAILED, errors));
        }

        var user = await _userManager.GetUserAsync(User);
        var res  = await _userManager.ChangePasswordAsync(user!, dto.OldPassword, dto.NewPassword);
        return res.Succeeded ? NoContent() : BadRequest(res.Errors.Select(e => e.Description));
    }
}