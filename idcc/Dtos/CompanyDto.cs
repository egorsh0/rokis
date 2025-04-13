namespace idcc.Dtos;

public record RegisterCompanyPayload(string FullName, string INN, string Email, string Password);

public record LoginCompanyPayload(string INNOrEmail, string Password);