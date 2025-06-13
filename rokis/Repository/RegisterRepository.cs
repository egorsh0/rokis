using rokis.Context;
using rokis.Dtos;
using rokis.Extensions;
using rokis.Infrastructures;
using rokis.Models;
using rokis.Models.Profile;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace rokis.Repository;

public interface IRegisterRepository
{
    // Регистрация
    Task<AuthResult> RegisterCompanyAsync(RegisterCompanyPayload dto);
    Task<AuthResult> RegisterEmployeeAsync(RegisterEmployeePayload dto);
    Task<AuthResult> RegisterPersonAsync(RegisterPersonPayload dto);
    
    Task<AuthResult> RegisterAdministratorAsync(RegisterAdministratorPayload dto);

    // Логин
    Task<(MessageCode code, ApplicationUser? applicationUser)> LoginCheckAsync(LoginPayload dto);
    
    Task<ApplicationUser?> FindUserAsync(string userId);
}

public class RegisterRepository : IRegisterRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly RokisContext _rokisContext;

    public RegisterRepository(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        RokisContext rokisContext)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _rokisContext = rokisContext;
    }

    // ---------------------------------------------------
    // РЕГИСТРАЦИЯ (компания, сотрудник, физ. лицо)
    // ---------------------------------------------------
    public async Task<AuthResult> RegisterCompanyAsync(RegisterCompanyPayload dto)
    {
        var errors = new List<string>();
        
        // 1. Убедимся, что роль "Company" существует
        if (!await _roleManager.RoleExistsAsync("Company"))
        {
            var roleRes = await _roleManager.CreateAsync(new IdentityRole("Company"));
            if (!roleRes.Succeeded)
            {
                // Формируем список ошибок
                errors = roleRes.Errors.Select(e => e.Description).ToList();
                return new AuthResult
                {
                    Succeeded = false,
                    MessageCode = MessageCode.ROLE_NOT_FOUND,
                    Errors = errors
                };
            }
        }
        
        // 1.1. Проверки email и INN при регистрации.
        
        // — проверка Email —
        if (await _rokisContext.Users.AnyAsync(u => u.Email == dto.Email))
        {
            errors.Add(MessageCode.EMAIL_ALREADY_EXISTS.GetDescription());
        }

        // — проверка ИНН —
        if (await _rokisContext.CompanyProfiles.AnyAsync(c => c.INN == dto.INN))
        {
            errors.Add(MessageCode.INN_ALREADY_EXISTS.GetDescription());
        }

        if (errors.Count > 0)
        {
            return new AuthResult
            {
                Succeeded = false,
                MessageCode = MessageCode.EMAIL_OR_INN_ALREADY_EXISTS,
                Errors = errors
            };
        }
        

        // 2. Создаём пользователя (Identity)
        var user = new ApplicationUser
        {
            Email = dto.Email,
            UserName = dto.Email
        };
        
        var createRes = await _userManager.CreateAsync(user, dto.Password);
        if (!createRes.Succeeded)
        {
            // Формируем список ошибок
            errors = createRes.Errors.Select(e => e.Description).ToList();
            return new AuthResult
            {
                Succeeded = false,
                MessageCode = MessageCode.REGISTER_HAS_ERRORS,
                Errors = errors
            };
        }

        // 3. Присваиваем роль
        await _userManager.AddToRoleAsync(user, "Company");

        // 4. Создаём профиль компании
        var companyProfile = new CompanyProfile
        {
            FullName = dto.FullName,
            INN = dto.INN,
            Email = dto.Email,
            UserId = user.Id
        };
        _rokisContext.CompanyProfiles.Add(companyProfile);
        await _rokisContext.SaveChangesAsync();

        return new AuthResult
        {
            Succeeded = true,
            MessageCode = MessageCode.REGISTER_IS_FINISHED,
            UserId = user.Id
        };
    }

    public async Task<AuthResult> RegisterEmployeeAsync(RegisterEmployeePayload dto)
    {
        var errors = new List<string>();
        // 1. Роль "Employee"
        if (!await _roleManager.RoleExistsAsync("Employee"))
        {
            var roleRes = await _roleManager.CreateAsync(new IdentityRole("Employee"));
            if (!roleRes.Succeeded)
            {
                // Формируем список ошибок
                errors = roleRes.Errors.Select(e => e.Description).ToList();
                return new AuthResult
                {
                    Succeeded = false,
                    MessageCode = MessageCode.ROLE_NOT_FOUND,
                    Errors = errors
                };
            }
        }
        
        // 1.5. Проверка Email
        // ── проверка Email ─────────────────────────────────────
        if (await _rokisContext.Users.AnyAsync(u => u.Email == dto.Email))
        {
            errors.Add(MessageCode.EMAIL_ALREADY_EXISTS.GetDescription());
        }

        if (errors.Count > 0)
        {
            return new AuthResult
            {
                Succeeded = false,
                MessageCode = MessageCode.EMAIL_ALREADY_EXISTS,
                Errors = errors
            };
        }

        // 2. Создаём ApplicationUser
        var user = new ApplicationUser
        {
            Email = dto.Email,
            UserName = dto.Email
        };
        var createRes = await _userManager.CreateAsync(user, dto.Password);
        if (!createRes.Succeeded)
        {
            // Формируем список ошибок
            errors = createRes.Errors.Select(e => e.Description).ToList();
            return new AuthResult
            {
                Succeeded = false,
                MessageCode = MessageCode.REGISTER_HAS_ERRORS,
                Errors = errors
            };
        }

        await _userManager.AddToRoleAsync(user, "Employee");

        // 3. Создаём профиль
        var employeeProfile = new EmployeeProfile
        {
            FullName = dto.FullName,
            Email = dto.Email,
            UserId = user.Id
        };

        bool linked   = false;
        CompanyInfoDto? companyDto = null;

        // ---------- 4.  Пытаемся найти компанию по INN или email ----------
        if (!string.IsNullOrWhiteSpace(dto.CompanyIdentifier))
        {
            var identifier = dto.CompanyIdentifier.Trim();
            // 4.1 Поиск по INN
            var companyProfile =
                await _rokisContext.CompanyProfiles
                    // 4.2 Если не нашли — поиск по email
                    .FirstOrDefaultAsync(cp => cp.INN == identifier) ?? await _rokisContext.CompanyProfiles
                    .FirstOrDefaultAsync(cp => cp.Email == identifier);

            if (companyProfile != null)
            {
                employeeProfile.CompanyProfileId = companyProfile.Id;
                linked  = true;
                companyDto = new CompanyInfoDto(companyProfile.FullName, companyProfile.INN, companyProfile.Email);
            }
        }
                    
        _rokisContext.EmployeeProfiles.Add(employeeProfile);
        await _rokisContext.SaveChangesAsync();

        return new AuthResult
        {
            Succeeded = true,
            MessageCode = MessageCode.REGISTER_IS_FINISHED,
            UserId = user.Id,
            LinkedToCompany = linked,
            Company = companyDto
        };
    }

    public async Task<AuthResult> RegisterPersonAsync(RegisterPersonPayload dto)
    {
        var errors = new List<string>();
        
        // 1. Роль "Person"
        if (!await _roleManager.RoleExistsAsync("Person"))
        {
            var roleRes = await _roleManager.CreateAsync(new IdentityRole("Person"));
            if (!roleRes.Succeeded)
            {
                // Формируем список ошибок
                errors = roleRes.Errors.Select(e => e.Description).ToList();
                return new AuthResult
                {
                    Succeeded = false,
                    MessageCode = MessageCode.ROLE_NOT_FOUND,
                    Errors = errors
                };
            }
        }
        
        // 1.5. Проверка Email

        if (await _rokisContext.Users.AnyAsync(u => u.Email == dto.Email))
        {
            errors.Add(MessageCode.EMAIL_ALREADY_EXISTS.GetDescription());
        }

        if (errors.Count > 0)
        {
            return new AuthResult
            {
                Succeeded = false,
                MessageCode = MessageCode.EMAIL_ALREADY_EXISTS,
                Errors = errors
            };
        }
        
        // 2. Создаём пользователя
        var user = new ApplicationUser
        {
            Email = dto.Email,
            UserName = dto.Email
        };
        
        var createRes = await _userManager.CreateAsync(user, dto.Password);
        if (!createRes.Succeeded)
        {
            // Формируем список ошибок
            errors = createRes.Errors.Select(e => e.Description).ToList();
            return new AuthResult
            {
                Succeeded = false,
                MessageCode = MessageCode.REGISTER_HAS_ERRORS,
                Errors = errors
            };
        }

        await _userManager.AddToRoleAsync(user, "Person");

        // 3. Профиль
        var personProfile = new PersonProfile
        {
            FullName = dto.FullName,
            Email = dto.Email,
            UserId = user.Id
        };
        _rokisContext.PersonProfiles.Add(personProfile);
        await _rokisContext.SaveChangesAsync();

        return new AuthResult
        {
            Succeeded = true,
            MessageCode = MessageCode.REGISTER_IS_FINISHED,
            UserId = user.Id
        };
    }
    
    public async Task<AuthResult> RegisterAdministratorAsync(RegisterAdministratorPayload dto)
    {
        // 1. Роль "Administrator"
        if (!await _roleManager.RoleExistsAsync("Administrator"))
        {
            var roleRes = await _roleManager.CreateAsync(new IdentityRole("Administrator"));
            if (!roleRes.Succeeded)
            {
                // Формируем список ошибок
                var errors = roleRes.Errors.Select(e => e.Description).ToList();
                return new AuthResult
                {
                    Succeeded = false,
                    Errors = errors
                };
            }
        }

        // 2. Создаём пользователя
        var user = new ApplicationUser
        {
            Email = dto.Email,
            UserName = dto.Email
        };
        
        var createRes = await _userManager.CreateAsync(user, dto.Password);
        if (!createRes.Succeeded)
        {
            // Формируем список ошибок
            var errors = createRes.Errors.Select(e => e.Description).ToList();
            return new AuthResult
            {
                Succeeded = false,
                Errors = errors
            };
        }

        await _userManager.AddToRoleAsync(user, "Administrator");

        // 3. Профиль
        var administratorProfile = new AdministratorProfile
        {
            Email = dto.Email,
            UserId = user.Id
        };
        _rokisContext.AdministratorProfiles.Add(administratorProfile);
        await _rokisContext.SaveChangesAsync();

        return new AuthResult
        {
            Succeeded = true,
            UserId = user.Id
        };
    }

    public async Task<(MessageCode, ApplicationUser?)> LoginCheckAsync(LoginPayload dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            return (MessageCode.USER_NOT_FOUND, null);
        }

        var validPass = await _userManager.CheckPasswordAsync(user, dto.Password);
        return !validPass ? (MessageCode.INVALID_PASSWORD, null) : (MessageCode.LOGIN_FINISHED, user);
    }
    
    public async Task<ApplicationUser?> FindUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user ?? null;
    }
}