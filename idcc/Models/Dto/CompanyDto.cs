namespace idcc.Models.Dto;

public record RegisterCompanyDto(string Name, string Inn, string Email, string Password);

public record CompanyDto(int Id, string? Name, string Inn, string Email);

public record SearchCompanyPayload(string Inn, string Email);

public record LoginCompanyPayload(string Email, string Inn, string Password);