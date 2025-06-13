using System.ComponentModel.DataAnnotations;

namespace rokis.Dtos;

public record RegisterAdministratorPayload(string Email, string Password);
public record RegisterCompanyPayload(string FullName, string INN, string Email, string Password);
public record RegisterEmployeePayload(string FullName, string Email, string Password, string? CompanyIdentifier);
public record RegisterPersonPayload(string FullName, string Email, string Password);

public record LoginPayload(string Email, string Password);

/// <summary>Запрос «забыли пароль».</summary>
/// <param name="Email">
/// Email учётной записи.<br/>
/// Должен существовать в системе; в противном случае API вернёт <c>404</c>.
/// </param>
/// <param name="BaseUrl">
/// BaseUrl фронт сервера.<br/>
/// </param>
public record ForgotPasswordDto(
    [EmailAddress] string Email,
    string BaseUrl);

/// <summary>Задать новый пароль по токену сброса.</summary>
/// <param name="UserId">
/// Идентификатор пользователя (строка GUID из AspNetUsers.Id).
/// Передаётся в query-строке из письма.
/// </param>
/// <param name="Token">
/// Токен сброса пароля (Base64Url). Получен из ссылки в email.
/// </param>
/// <param name="NewPassword">Новый пароль пользователя.</param>
/// <param name="ConfirmPassword">
/// Повтор нового пароля. Значения <c>NewPassword</c> и <c>ConfirmPassword</c>
/// должны совпадать, иначе API вернёт <c>400 BadRequest</c>.
/// </param>
public record ResetPasswordDto(
    string UserId,
    string Token,
    string NewPassword,
    string ConfirmPassword);