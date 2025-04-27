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

namespace MyProject.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuthRepository _authRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        IAuthRepository authRepository, 
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _authRepository = authRepository;
        _configuration = configuration;
        _logger = logger;
    }

        // ------------------ РЕГИСТРАЦИЯ ---------------------
    [HttpPost("register/company")]
    public async Task<IActionResult> RegisterCompany([FromBody] RegisterCompanyPayload dto)
    {
        try
        {
            var result = await _authRepository.RegisterCompanyAsync(dto);
            if (!result.Succeeded)
            {
                // Возвращаем BadRequest + список ошибок
                return BadRequest(new { result.Errors });
            }
            _logger.LogInformation("Registered company userId={UserId}, INN={INN}", result.UserId, dto.INN);
            return Ok("Company registered successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering company");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("register/employee")]
    public async Task<IActionResult> RegisterEmployee([FromBody] RegisterEmployeePayload dto)
    {
        try
        {
            var result = await _authRepository.RegisterEmployeeAsync(dto);
            if (!result.Succeeded)
            {
                // Возвращаем BadRequest + список ошибок
                return BadRequest(new { result.Errors });
            }
            _logger.LogInformation("Registered employee userId={UserId}", result.UserId);
            return Ok("Employee registered successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering employee");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("register/person")]
    public async Task<IActionResult> RegisterPerson([FromBody] RegisterPersonPayload dto)
    {
        try
        {
            var result = await _authRepository.RegisterPersonAsync(dto);
            if (!result.Succeeded)
            {
                // Возвращаем BadRequest + список ошибок
                return BadRequest(new { result.Errors });
            }
            _logger.LogInformation("Registered person userId={UserId}", result.UserId);
            return Ok("Person registered successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering person");
            return StatusCode(500, "Internal server error");
        }
    }

    // ------------------ ЛОГИН ---------------------
    [HttpPost("login/company")]
    public async Task<IActionResult> LoginCompany([FromBody] LoginCompanyPayload dto)
    {
        try
        {
            var user = await _authRepository.LoginCompanyAsync(dto);
            if (user == null)
            {
                return Unauthorized("Invalid credentials");
            }
            // Генерируем JWT, если нужно
            var token = await GenerateJwtTokenAsync(user);
            return Ok(new { token });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error login company");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("login/employee")]
    public async Task<IActionResult> LoginEmployee([FromBody] LoginEmployeePayload dto)
    {
        try
        {
            var user = await _authRepository.LoginEmployeeAsync(dto);
            if (user == null)
            {
                return Unauthorized("Invalid credentials");
            }
            var token = await GenerateJwtTokenAsync(user);
            return Ok(new { token });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error login employee");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("login/person")]
    public async Task<IActionResult> LoginPerson([FromBody] LoginPersonPayload dto)
    {
        try
        {
            var user = await _authRepository.LoginPersonAsync(dto);
            if (user == null)
            {
                return Unauthorized("Invalid credentials");
            }
            var token = await GenerateJwtTokenAsync(user);
            return Ok(new { token });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error login person");
            return StatusCode(500, "Internal server error");
        }
    }
    
    /// <summary>
    /// Возвращает сведения о текущем пользователе (идентификатор, email, роли).
    /// Только для авторизованных (Authorize).
    /// </summary>
    [HttpGet("whoami")]
    [Authorize]
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
    private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
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

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}