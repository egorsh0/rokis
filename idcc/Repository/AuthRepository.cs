using idcc.Context;
using idcc.Dtos;
using idcc.Models;
using idcc.Models.Profile;
using idcc.Repository.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace idcc.Repository;

public class AuthRepository : IAuthRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IdccContext _idccContext;

    public AuthRepository(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IdccContext idccContext)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _idccContext = idccContext;
    }

    // ---------------------------------------------------
    // РЕГИСТРАЦИЯ (компания, сотрудник, физ. лицо)
    // ---------------------------------------------------
    public async Task<AuthResult> RegisterCompanyAsync(RegisterCompanyPayload dto)
    {
        // 1. Убедимся, что роль "Company" существует
        if (!await _roleManager.RoleExistsAsync("Company"))
        {
            var roleRes = await _roleManager.CreateAsync(new IdentityRole("Company"));
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
            var errors = createRes.Errors.Select(e => e.Description).ToList();
            return new AuthResult
            {
                Succeeded = false,
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
        _idccContext.CompanyProfiles.Add(companyProfile);
        await _idccContext.SaveChangesAsync();

        return new AuthResult
        {
            Succeeded = true,
            UserId = user.Id
        };
    }

    public async Task<AuthResult> RegisterEmployeeAsync(RegisterEmployeePayload dto)
    {
        // 1. Роль "Employee"
        if (!await _roleManager.RoleExistsAsync("Employee"))
        {
            var roleRes = await _roleManager.CreateAsync(new IdentityRole("Employee"));
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
            var errors = createRes.Errors.Select(e => e.Description).ToList();
            return new AuthResult
            {
                Succeeded = false,
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

        // Если сразу хотим привязать к компании (опционально):
        if (!string.IsNullOrEmpty(dto.CompanyUserId))
        {
            var company = await _idccContext.CompanyProfiles
                .FirstOrDefaultAsync(cp => cp.UserId == dto.CompanyUserId);
            if (company != null)
            {
                employeeProfile.CompanyProfileId = company.Id;
            }
        }

        _idccContext.EmployeeProfiles.Add(employeeProfile);
        await _idccContext.SaveChangesAsync();

        return new AuthResult
        {
            Succeeded = true,
            UserId = user.Id
        };
    }

    public async Task<AuthResult> RegisterPersonAsync(RegisterPersonPayload dto)
    {
        // 1. Роль "Person"
        if (!await _roleManager.RoleExistsAsync("Person"))
        {
            var roleRes = await _roleManager.CreateAsync(new IdentityRole("Person"));
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

        await _userManager.AddToRoleAsync(user, "Person");

        // 3. Профиль
        var personProfile = new PersonProfile
        {
            FullName = dto.FullName,
            Email = dto.Email,
            UserId = user.Id
        };
        _idccContext.PersonProfiles.Add(personProfile);
        await _idccContext.SaveChangesAsync();

        return new AuthResult
        {
            Succeeded = true,
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
        _idccContext.AdministratorProfiles.Add(administratorProfile);
        await _idccContext.SaveChangesAsync();

        return new AuthResult
        {
            Succeeded = true,
            UserId = user.Id
        };
    }

    // ---------------------------------------------------
    // ЛОГИН (компания, сотрудник, физ. лицо)
    // ---------------------------------------------------
    public async Task<ApplicationUser?> LoginCompanyAsync(LoginCompanyPayload dto)
    {
        // 1. Ищем по Email
        var user = await _userManager.FindByEmailAsync(dto.INNOrEmail);
        // 2. Если не нашли, ищем в CompanyProfile по INN
        if (user == null)
        {
            var companyProfile = await _idccContext.CompanyProfiles
                .FirstOrDefaultAsync(cp => cp.INN == dto.INNOrEmail);
            if (companyProfile != null)
            {
                user = await _userManager.FindByIdAsync(companyProfile.UserId);
            }
        }
        if (user == null) return null;

        // Проверяем, что он "Company"
        var isCompany = await _userManager.IsInRoleAsync(user, "Company");
        if (!isCompany) return null;

        // Проверяем пароль
        var validPass = await _userManager.CheckPasswordAsync(user, dto.Password);
        return !validPass ? null : user;
    }

    public async Task<ApplicationUser?> LoginEmployeeAsync(LoginEmployeePayload dto)
    {
        // Сотрудник заходит только по Email
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null) return null;

        var isEmployee = await _userManager.IsInRoleAsync(user, "Employee");
        if (!isEmployee) return null;

        var validPass = await _userManager.CheckPasswordAsync(user, dto.Password);
        return !validPass ? null : user;
    }

    public async Task<ApplicationUser?> LoginPersonAsync(LoginPersonPayload dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null) return null;

        var isPerson = await _userManager.IsInRoleAsync(user, "Person");
        if (!isPerson) return null;

        var validPass = await _userManager.CheckPasswordAsync(user, dto.Password);
        return !validPass ? null : user;
    }
    
    public async Task<ApplicationUser?> LoginAdministratorAsync(LoginAdministratorPayload dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null) return null;

        var isAdministrator = await _userManager.IsInRoleAsync(user, "Administrator");
        if (!isAdministrator) return null;

        var validPass = await _userManager.CheckPasswordAsync(user, dto.Password);
        return !validPass ? null : user;
    }
}