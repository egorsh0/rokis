using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using idcc.Context;
using idcc.Dtos;
using idcc.Models;
using idcc.Models.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace MyProject.Controllers;

[ApiController]
[Route("api/v1")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IdccContext _idccContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IdccContext idccContext,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _idccContext = idccContext;
        _configuration = configuration;
        _logger = logger;
    }

    // ------------------------------------------------------------
    // РЕГИСТРАЦИЯ КОМПАНИИ
    // ------------------------------------------------------------
    [HttpPost("register/company")]
    public async Task<IActionResult> RegisterCompany([FromBody] RegisterCompanyPayload dto)
    {
        try
        {
            if (!await _roleManager.RoleExistsAsync("Company"))
            {
                var res = await _roleManager.CreateAsync(new IdentityRole("Company"));
                if (!res.Succeeded)
                    return StatusCode(500, "Не удалось создать роль 'Company'");
            }

            var user = new ApplicationUser
            {
                Email = dto.Email,
                UserName = dto.Email
            };
            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
                return BadRequest(createResult.Errors);

            await _userManager.AddToRoleAsync(user, "Company");

            // Создаём профиль компании
            var companyProfile = new CompanyProfile
            {
                UserId = user.Id,
                OrganizationName = dto.OrganizationName,
                INN = dto.INN
            };
            _idccContext.CompanyProfiles.Add(companyProfile);
            await _idccContext.SaveChangesAsync();

            _logger.LogInformation("Company registered. UserId={UserId}, INN={INN}", user.Id, dto.INN);
            return Ok(new { message = "Company registered", userId = user.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при регистрации компании");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    // ------------------------------------------------------------
    // РЕГИСТРАЦИЯ СОТРУДНИКА (EMPLOYEE)
    // ------------------------------------------------------------
    [HttpPost("register/employee")]
    public async Task<IActionResult> RegisterEmployee([FromBody] RegisterEmployeePayload dto)
    {
        try
        {
            if (!await _roleManager.RoleExistsAsync("Employee"))
            {
                var res = await _roleManager.CreateAsync(new IdentityRole("Employee"));
                if (!res.Succeeded)
                    return StatusCode(500, "Не удалось создать роль 'Employee'");
            }

            // Проверяем существование компании
            var companyUser = await _userManager.FindByIdAsync(dto.CompanyUserId);
            if (companyUser == null)
                return BadRequest($"CompanyUserId '{dto.CompanyUserId}' не найден.");

            // Проверяем, что у companyUser роль "Company"
            var isCompany = await _userManager.IsInRoleAsync(companyUser, "Company");
            if (!isCompany)
                return BadRequest($"Пользователь '{dto.CompanyUserId}' не является компанией.");

            // Создаём ApplicationUser
            var user = new ApplicationUser
            {
                Email = dto.Email,
                UserName = dto.Email,
                Name = dto.Name
            };
            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
                return BadRequest(createResult.Errors);

            // Назначаем роль
            await _userManager.AddToRoleAsync(user, "Employee");

            // Создаём единый профиль (UserProfile)
            var userProfile = new UserProfile
            {
                UserId = user.Id,
                CompanyUserId = dto.CompanyUserId // указываем, в какой компании работает
            };
            _idccContext.UserProfiles.Add(userProfile);
            await _idccContext.SaveChangesAsync();

            _logger.LogInformation("Employee registered. UserId={UserId}, CompanyUserId={CompanyId}", 
                user.Id, dto.CompanyUserId);
            return Ok(new { message = "Employee registered", userId = user.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при регистрации сотрудника");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    // ------------------------------------------------------------
    // РЕГИСТРАЦИЯ ФИЗ. ЛИЦА (PERSON)
    // ------------------------------------------------------------
    [HttpPost("register/person")]
    public async Task<IActionResult> RegisterPerson([FromBody] RegisterPersonPayload dto)
    {
        try
        {
            if (!await _roleManager.RoleExistsAsync("Person"))
            {
                var res = await _roleManager.CreateAsync(new IdentityRole("Person"));
                if (!res.Succeeded)
                    return StatusCode(500, "Не удалось создать роль 'Person'");
            }

            var user = new ApplicationUser
            {
                Email = dto.Email,
                UserName = dto.Email,
                Name = dto.Name
            };
            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
                return BadRequest(createResult.Errors);

            await _userManager.AddToRoleAsync(user, "Person");

            // Создаём тот же UserProfile, но без CompanyUserId
            var userProfile = new UserProfile
            {
                UserId = user.Id
            };
            _idccContext.UserProfiles.Add(userProfile);
            await _idccContext.SaveChangesAsync();

            _logger.LogInformation("Person registered. UserId={UserId}", user.Id);
            return Ok(new { message = "Person registered", userId = user.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при регистрации физ. лица");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    // ------------------------------------------------------------
    // ЛОГИН КОМПАНИИ (по INN или Email)
    // ------------------------------------------------------------
    [HttpPost("login/company")]
    public async Task<IActionResult> LoginCompany([FromBody] LoginCompanyPayload dto)
    {
        try
        {
            // 1) Сначала ищем пользователя по Email
            var user = await _userManager.FindByEmailAsync(dto.INNOrEmail);

            // 2) Если не нашли, ищем по INN в CompanyProfile (пример)
            if (user == null)
            {
                // Предположим, что CompanyProfile хранит INN и UserId
                var companyProfile = await _idccContext.CompanyProfiles
                    .FirstOrDefaultAsync(cp => cp.INN == dto.INNOrEmail);

                if (companyProfile != null)
                {
                    user = await _userManager.FindByIdAsync(companyProfile.UserId);
                }
            }

            if (user == null)
            {
                _logger.LogWarning("Company login failed: no user found for '{Value}'", dto.INNOrEmail);
                return Unauthorized("Invalid credentials");
            }

            // Проверяем, что это действительно "Company"
            bool isCompany = await _userManager.IsInRoleAsync(user, "Company");
            if (!isCompany)
            {
                _logger.LogWarning("Attempt to login as company but user is not 'Company'. UserId={UserId}", user.Id);
                return Unauthorized("User is not a company");
            }

            // Проверяем пароль
            var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordValid)
            {
                _logger.LogWarning("Invalid password for CompanyUserId={UserId}", user.Id);
                return Unauthorized("Invalid credentials");
            }

            // Генерируем JWT
            var token = await GenerateJwtTokenAsync(user);

            _logger.LogInformation("Company login success: UserId={UserId}", user.Id);
            // Возвращаем { token = "...jwt..." }
            return Ok(new { token });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при логине компании");
            return StatusCode(500, "Внутренняя ошибка сервера");
        }
    }

    // ------------------------------------------------------------
    // ЛОГИН ДЛЯ СОТРУДНИКОВ (EMPLOYEE) и ФИЗ. ЛИЦ (PERSON)
    // ------------------------------------------------------------
    [HttpPost("login/user")]
    public async Task<IActionResult> LoginUser([FromBody] LoginUserPayload dto)
    {
        try
        {
            // Ищем по Email
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                _logger.LogWarning("User login failed: no user found for Email={Email}", dto.Email);
                return Unauthorized("Invalid credentials");
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordValid)
            {
                _logger.LogWarning("Invalid password for UserId={UserId}", user.Id);
                return Unauthorized("Invalid credentials");
            }

            // Смотрим, есть ли у пользователя роль "Employee" или "Person"
            var isEmployee = await _userManager.IsInRoleAsync(user, "Employee");
            var isPerson = await _userManager.IsInRoleAsync(user, "Person");
            if (!isEmployee && !isPerson)
            {
                _logger.LogWarning("User {UserId} is neither Employee nor Person", user.Id);
                return Unauthorized("User is neither employee nor person");
            }
            
            // Генерируем JWT
            var token = await GenerateJwtTokenAsync(user);

            _logger.LogInformation("User login success: UserId={UserId}, Email={Email}", user.Id, dto.Email);
            return Ok(new { message = "User login success", token });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при логине сотрудника/физ. лица");
            return StatusCode(500, "Внутренняя ошибка сервера");
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
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? "")
        };

        // Добавим роли в токен:
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Далее ваши обычные шаги:
        var secret = _configuration["Jwt:Secret"];
        var key = Encoding.UTF8.GetBytes(secret);
        var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
