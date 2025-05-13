using System.ComponentModel.DataAnnotations;

namespace idcc.Dtos;

public record StartSessionDto([property: Required] Guid TokenId);
public record SessionResultDto(int? Id, Guid TokenId, bool Succeeded, string? ErrorMessage);

public record SessionDto(
    int         Id,
    DateTime    StartTime,
    DateTime?   EndTime,
    double      Score,
    TokenShortDto Token);