namespace idcc.Dtos;

public record RegisterEmployeePayload(string FullName, string Email, string Password, string? CompanyIdentifier);

public record LoginEmployeePayload(string Email, string Password);