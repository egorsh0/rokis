using System.ComponentModel.DataAnnotations;
using idcc.Infrastructures;

namespace idcc.Dtos;

public record TokenDto(Guid Id, int DirectionId, string DirectionName, decimal UnitPrice, TokenStatus Status, string? BoundFullName, string? BoundEmail, DateTime? UsedDate, string? CertificateUrl);
public record BindTokenDto(Guid TokenId, [property: Required, EmailAddress] string EmployeeEmail);
public record BindUsedTokenDto(Guid TokenId, [property: Required, EmailAddress] string UserEmail);
public record TokenShortDto([property: Required] Guid Id, int DirectionId, string DirectionName, TokenStatus Status);