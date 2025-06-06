using idcc.Infrastructures;

namespace idcc.Dtos;

/// <summary>
/// Итоговый отчет.
/// </summary>
public record ReportDto(
    Guid TokenId,
    DateTime StartSession,
    DateTime EndSession,
    TimeSpan TestingTime,
    double CognitiveStabilityIndex,
    ThinkingPattern ThinkingPattern,
    FinalScoreDto? FinalScoreDto,
    List<FinalTopicData>? FinalTopicDatas);

public record ReportShortDto(
    Guid    TokenId,
    double  Score,
    string  Grade,
    double CognitiveStabilityIndex,
    ThinkingPattern ThinkingPattern,
    string? ImageBase64); 

/// <summary>
/// Итоговая оценка.
/// </summary>
public record FinalScoreDto(double Score, string Grade);

/// <summary>
/// Итог по темам.
/// </summary>
public record FinalTopicData(string Topic, double Score, string Grade, int Positive, int Negative);

/// <summary>Ответ «generate»: полный отчёт + (опц.) картинка.</summary>
public record ReportGeneratedDto(ReportDto Report, string? ImageBase64, string? AnotherImageBase64, string? OldImageBase64);