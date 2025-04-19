namespace idcc.Dtos;

public record RegisterAdministratorPayload(string Email, string Password);

public record LoginAdministratorPayload(string Email, string Password);