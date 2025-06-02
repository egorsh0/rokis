namespace idcc.Dtos;

/// <summary>
/// Итоговый отчет.
/// </summary>
public record ReportDto(
    Guid TokenId,
    DateTime StartSession,
    DateTime EndSession,
    TimeSpan TestingTime,
    FinalScoreDto? FinalScoreDto,
    List<FinalTopicData>? FinalTopicDatas);

public record ReportShortDto(
    Guid    TokenId,
    double  Score,
    string  Grade,
    string? ImageBase64); 

/// <summary>
/// Итоговая оценка.
/// </summary>
public record FinalScoreDto(double Score, string Grade);


/// <summary>
/// Итог по темам.
/// </summary>
public record FinalTopicData(string Topic, double Score, int Positive, int Negative);

/// <summary>Ответ «generate»: полный отчёт + (опц.) картинка.</summary>
public record ReportGeneratedDto(ReportDto Report, string? ImageBase64);