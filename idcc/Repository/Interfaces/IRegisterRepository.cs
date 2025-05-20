using idcc.Dtos;
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
    Task<ApplicationUser?> LoginAsync(LoginPayload dto);
}