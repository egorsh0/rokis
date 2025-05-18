using System.Security.Claims;
using idcc.Dtos;
using idcc.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace idcc.Endpoints;

/// <summary>Методы, доступные физ лицу-владельцу токенов.</summary>
[ApiController]
[Route("api/person")]
[Authorize(Roles = "Person")]
public class PersonController : ControllerBase
{
    private readonly IPersonRepository _personRepository;
    private readonly ILogger<PersonController> _logger;

    public PersonController(IPersonRepository personRepository,
        ILogger<PersonController> logger)
    {
        _personRepository = personRepository;
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
            return NotFound("Person not found");
        }

        var dto = new PersonProfileDto(
            person.Id,
            person.FullName,
            person.Email);

        return Ok(dto);
    }
}