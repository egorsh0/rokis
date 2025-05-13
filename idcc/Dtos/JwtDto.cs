namespace idcc.Dtos;

/// <summary>Ответ авторизации — содержит сгенерированный JWT.</summary>
public record JwtResponse(string Token);