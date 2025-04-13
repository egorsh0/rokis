namespace idcc.Dtos;

public record RegisterPersonPayload(string FullName, string Email, string Password);

public record LoginPersonPayload(string Email, string Password);