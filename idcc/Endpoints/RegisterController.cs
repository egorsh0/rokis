using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using idcc.Dtos;
using idcc.Models;
using idcc.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace idcc.Endpoints;

/// <summary>Регистрация и аутентификация пользователей (Company / Employee / Person).</summary>
[ApiController]
[Route("api/register")]
public class RegisterController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRegisterRepository _registerRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RegisterController> _logger;

    
    public RegisterController(
        UserManager<ApplicationUser> userManager,
        IRegisterRepository registerRepository, 
        IConfiguration configuration,
        ILogger<RegisterController> logger)
    {
        _userManager = userManager;
        _registerRepository = registerRepository;
        _configuration = configuration;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════
    //     POST /auth/register/company
    // ═══════════════════════════════════════════════════════
    /// <summary>Регистрация компании.</summary>
    /// <remarks>
    /// **Сценарий:** вызывается при создании новой компании.<br/>
    /// При успехе — <c>200 OK</c>, иначе <c>409</c> со списком ошибок Identity.
    /// <response code="409">
    /// EMAIL_ALREADY_EXISTS — почта уже зарегистрирована.<br/>
    /// INN_ALREADY_EXISTS   — компания с таким ИНН существует.
    /// </response>
    /// </remarks>
    /// <param name="dto">JSON с реквизитами компании.</param>
    /// <response code="200">Компания зарегистрирована.</response>
    /// <response code="409">Ошибка валидации (например, пароль слабый).</response>
    /// <response code="500">Непредвиденная ошибка сервера.</response>
    [HttpPost("company")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterCompany([FromBody] RegisterCompanyPayload dto)
    {
        try
        {
            var result = await _registerRepository.RegisterCompanyAsync(dto);
            if (!result.Succeeded)
            {
                return Conflict(new { result.Errors });
            }
            _logger.LogInformation("Registered company userId={UserId}, INN={INN}", result.UserId, dto.INN);
            return Ok(new ResponseDto("Company registered successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering company");
            return StatusCode(500, new ResponseDto("Internal server error"));
        }
    }

    /// <summary>Регистрация сотрудника компании.</summary>
    /// <remarks>
    /// При передаче <c>companyIdentifier</c> (ИНН или email компании)
    /// сотрудник сразу связывается с этой компанией.
    /// <response code="409">
    /// EMAIL_ALREADY_EXISTS — пользователь с таким email уже есть.
    /// </response>
    /// </remarks>
    /// <response code="200">Сотрудник зарегистрирован.</response>
    /// <response code="409">Ошибки Identity.</response>
    [HttpPost("employee")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterEmployee([FromBody] RegisterEmployeePayload dto)
    {
        try
        {
            var result = await _registerRepository.RegisterEmployeeAsync(dto);
            if (!result.Succeeded)
            {
                return Conflict(new { result.Errors });
            }
            _logger.LogInformation("Registered employee userId={UserId}", result.UserId);
            return Ok(new ResponseDto("Employee registered successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering employee");
            return StatusCode(500, new ResponseDto("Internal server error"));
        }
    }

    /// <summary>Регистрация физического лица.</summary>
    /// <remarks>
    /// <response code="409">
    /// EMAIL_ALREADY_EXISTS — почта уже используется.
    /// </response>
    /// </remarks>
    /// <response code="200">Физ. лицо зарегистрировано.</response>
    /// <response code="409">Ошибки Identity.</response>
    [HttpPost("person")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterPerson([FromBody] RegisterPersonPayload dto)
    {
        try
        {
            var result = await _registerRepository.RegisterPersonAsync(dto);
            if (!result.Succeeded)
            {
                return Conflict(new { result.Errors });
            }
            _logger.LogInformation("Registered person userId={UserId}", result.UserId);
            return Ok(new ResponseDto("Person registered successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering person");
            return StatusCode(500, new ResponseDto("Internal server error"));
        }
    }

    /// <summary>Аутентификация компании, сотрудника, физ лица (email + пароль).</summary>
    /// <remarks>JWT c role=<c>Company/Employee/Person</c>.</remarks>
    /// <response code="200">Успешно.</response>
    /// <response code="401">Неверные данные.</response>
    [HttpPost("login")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(JwtResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string),      StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LoginEmployee([FromBody] LoginPayload dto)
    {
        try
        {
            var user = await _registerRepository.LoginAsync(dto);
            if (user == null)
            {
                return Unauthorized(new ResponseDto("Invalid credentials"));
            }
            var token = await GenerateJwtTokenAsync(user);
            return Ok(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error login employee");
            return StatusCode(500, new ResponseDto("Internal server error"));
        }
    }
    
    /// <summary>Текущий авторизованный пользователь.</summary>
    /// <response code="200">
    /// <example>
    /// { "userId":"…","email":"…","roles":["Employee"] }
    /// </example>
    /// </response>
    /// <response code="401">Если токен не передан или истёк.</response>
    [HttpGet("whoami")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult WhoAmI()
    {
        // Найдём NameIdentifier (AspNetUsers.Id), если он есть
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Часто в токен (или куки) записывают Email в claim c типом "email" или ClaimTypes.Email.
        // Может быть null, если вы не добавляете Email в токен.
        var email = User.FindFirstValue(ClaimTypes.Email);

        // Список ролей: ASP.NET Core сопоставляет ClaimTypes.Role = "role" (по умолчанию).
        var roles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        // Можно вытащить и другие клеймы – например, имя: User.FindFirstValue(ClaimTypes.Name)
        // или что-то кастомное, если вы туда записывали.

        // Сформируем результат
        var result = new 
        {
            UserId = userId,
            Email = email,
            Roles = roles
        };

        return Ok(result);
    }
    
    // ------------------------------------------------------------
    // Пример генерации JWT
    // ------------------------------------------------------------
    private async Task<LoginDto> GenerateJwtTokenAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? ""),
            new("IP", HttpContext.Connection.RemoteIpAddress?.ToString() ?? ""),
            new("UserAgent", Request.Headers["User-Agent"].ToString())
        };

        // Добавим роли в токен:
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Далее ваши обычные шаги:
        var secret = _configuration["Jwt:Secret"];
        var key = Encoding.UTF8.GetBytes(secret!);
        var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "Idcc",
            audience: "Idcc",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );
        var login = new LoginDto(new JwtSecurityTokenHandler().WriteToken(token), roles.ToList());
        return login;
    }
}