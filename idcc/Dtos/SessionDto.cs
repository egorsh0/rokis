namespace idcc.Dtos;

public record StartSessionDto(Guid TokenId);
public record SessionResultDto(int? Id, Guid TokenId, bool Succeeded, string? ErrorMessage);

public record SessionDto(
    int         Id,
    DateTime    StartTime,
    DateTime?   EndTime,
    double      Score,
    TokenShortDto Token);