using idcc.Infrastructures;

namespace idcc.Dtos;

public record TokenDto(Guid Id, int DirectionId, string DirectionName, decimal UnitPrice, TokenStatus Status, string? BoundFullName, string? BoundEmail, DateTime? UsedDate, string? CertificateUrl);
public record BindTokenDto(Guid TokenId, string EmployeeEmail);
public record BindUsedTokenDto(Guid TokenId, string UserEmail);
public record TokenShortDto(Guid Id, int DirectionId, string DirectionName, TokenStatus Status);