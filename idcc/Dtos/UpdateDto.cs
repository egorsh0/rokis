using System.ComponentModel.DataAnnotations;
using idcc.Infrastructures;

namespace idcc.Dtos;

public record UpdateCompanyDto(
    string? Name,
    string? LegalAddress,
    [EmailAddress] string? Email,
    string? Inn,
    string? Kpp);

public record UpdateEmployeeDto(
    string? FullName,
    [EmailAddress] string? Email);

public record UpdatePersonDto(
    string? FullName,
    [EmailAddress] string? Email);

public record UpdateResult(bool Succeeded, List<(MessageCode code, string message)> Errors);
    
/// <summary>Запрос на смену пароля.</summary>
public class ChangePasswordDto : IValidatableObject
{
    [Required(ErrorMessage = "OldPassword is required")]
    public string OldPassword { get; set; } = null!;

    [Required, MinLength(8)]
    public string NewPassword { get; set; } = null!;

    [Required]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
    public string ConfirmNewPassword { get; set; } = null!;

    /// <summary>Доп.кросс-валидация (если нужны свои правила).</summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (OldPassword == NewPassword)
            yield return new ValidationResult(
                "New password must differ from old one",
                [nameof(NewPassword)]);
    }
}