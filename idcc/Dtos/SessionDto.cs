using System.ComponentModel.DataAnnotations;
using idcc.Infrastructures;

namespace idcc.Dtos;

public record StartSessionDto([Required] Guid TokenId);
public record StopSessionDto(bool isSuccess, MessageCode Code, string Message);
public record SessionResultDto(int? Id, Guid TokenId, bool Succeeded, MessageCode Code, string? ErrorMessage)
{
    public TimeSpan? DurationTime { get; set; }
}

public record SessionDto(
    int         Id,
    DateTime    StartTime,
    DateTime?   EndTime,
    double      Score,
    TokenShortDto Token);