namespace idcc.Dtos;

public record RegisterAdministratorPayload(string Email, string Password);
public record RegisterCompanyPayload(string FullName, string INN, string Email, string Password);
public record RegisterEmployeePayload(string FullName, string Email, string Password, string? CompanyIdentifier);
public record RegisterPersonPayload(string FullName, string Email, string Password);

public record LoginPayload(string Email, string Password);