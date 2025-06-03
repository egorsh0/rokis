using idcc.Dtos;
using idcc.Infrastructures;
using idcc.Models;

namespace idcc.Repository.Interfaces;

public interface IRegisterRepository
{
    // Регистрация
    Task<AuthResult> RegisterCompanyAsync(RegisterCompanyPayload dto);
    Task<AuthResult> RegisterEmployeeAsync(RegisterEmployeePayload dto);
    Task<AuthResult> RegisterPersonAsync(RegisterPersonPayload dto);
    
    Task<AuthResult> RegisterAdministratorAsync(RegisterAdministratorPayload dto);

    // Логин
    Task<(MessageCode code, ApplicationUser? applicationUser)> LoginAsync(LoginPayload dto);
    
    Task<ApplicationUser?> FindUserAsync(string userId);
}