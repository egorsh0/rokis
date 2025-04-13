namespace idcc.Dtos;

public record RegisterCompanyPayload(string OrganizationName, string INN, string Email, string Password);

public record LoginCompanyPayload(string INNOrEmail, string Password);