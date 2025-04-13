namespace idcc.Dtos;

public record RegisterPersonPayload(string Name, string Email, string Password);

public record LoginUserPayload(string Email, string Password);