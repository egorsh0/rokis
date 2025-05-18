using System.ComponentModel.DataAnnotations;

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
    
public record ChangePasswordDto(
    [property: Required] string OldPassword,
    [property: Required] string NewPassword,
    [property: Required] string ConfirmNewPassword);