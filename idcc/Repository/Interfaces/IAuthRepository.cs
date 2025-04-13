using idcc.Dtos;
using idcc.Models;

namespace idcc.Repository.Interfaces;

public interface IAuthRepository
{
    // Регистрация
    Task<AuthResult> RegisterCompanyAsync(RegisterCompanyPayload dto);
    Task<AuthResult> RegisterEmployeeAsync(RegisterEmployeePayload dto);
    Task<AuthResult> RegisterPersonAsync(RegisterPersonPayload dto);

    // Логин
    Task<ApplicationUser?> LoginCompanyAsync(LoginCompanyPayload dto);
    Task<ApplicationUser?> LoginEmployeeAsync(LoginEmployeePayload dto);
    Task<ApplicationUser?> LoginPersonAsync(LoginPersonPayload dto);
}