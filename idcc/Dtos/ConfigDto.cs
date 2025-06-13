namespace idcc.Dtos;

/// <summary>
/// Время ответа на вопросы для расчёта коэффициента <c>K</c>
/// в зависимости от грейда.
/// </summary>
/// <param name="Grade">Название грейда (Junior / Middle / Senior…).</param>
/// <param name="Average">Среднее время ответа (сек).</param>
/// <param name="Min">Минимальное «нормативное» время (сек).</param>
/// <param name="Max">Максимальное «нормативное» время (сек).</param>
public record AnswerTimeDto(GradeDto Grade, double Average, double Min, double Max);

/// <summary>
/// Универсальный «счётчик» с произвольным кодом и числовым значением.
/// </summary>
/// <param name="Code">Код счётчика (например <c>"TotalQuestions"</c>).</param>
/// <param name="Description">Описание, для чего используется.</param>
/// <param name="Value">Целочисленное значение.</param>
public record CountDto(string Code, string Description, int Value);

/// <summary>Справочник грейдов.</summary>
public record GradeDto(
    int Id,
    string Name,        // Junior, Middle …
    string Code,        // JN, MD …
    string Description);

/// <summary>
/// Вес (коэффициент) уровня внутри грейда.<br/>
/// Используется при расчёте итогового результата.
/// </summary>
/// <param name="Grade">Название грейда.</param>
/// <param name="Min">Коэффициент уровня Min.</param>
/// <param name="Max">Коэффициент уровня Max.</param>
public record GradeLevelDto(GradeDto Grade, double Min, double Max);

/// <summary>
/// Диапазоны «начало–конец» для привязки результата к уровню грейда.
/// </summary>
/// <param name="Start">Нижняя граница %, включительно.</param>
/// <param name="End">Верхняя граница %, включительно.</param>
public record GradeRelationDto(string? Start, string? End);

/// <summary>Процентные значения для настроек системы.</summary>
public record PersentDto(string Code, string Description, double Value);

/// <summary>Временные настройки системы.</summary>
public record TimeDto(string Code, string Description, double Value);

/// <summary>
/// Направления, по которым продаются токены (QA, Dev, SA…).<br/>
/// Содержит базовую цену одного токена.
/// </summary>
public record DirectionDto(
    int Id,
    string Name,
    string Code,
    string Description,
    decimal BasePrice);

/// <summary>
/// Правило скидки: диапазон количества → ставка скидки.
/// </summary>
/// <param name="MinQuantity">Минимум токенов в заказе.</param>
/// <param name="MaxQuantity">
/// Максимум токенов (включительно).  
/// <c>null</c> — «до бесконечности».
/// </param>
/// <param name="DiscountRate">Ставка скидки (0.1 = -10 %).</param>
public record DiscountRuleDto(int MinQuantity, int? MaxQuantity, decimal DiscountRate);

/// <summary>Диапазон весов вопросов для конкретного грейда.</summary>
/// <param name="Id">Идентификатор грейда.</param>
/// <param name="Grade">Название грейда.</param>
/// <param name="Min">Min-вес вопроса.</param>
/// <param name="Max">Max-вес вопроса.</param>
public record WeightDto(int Id, GradeDto Grade, double Min, double Max);

/// <summary>
/// Настройка рассылки (cron-job / событие). Определяет, включена ли
/// рассылка и шаблоны письма.
/// </summary>
/// <param name="MailingCode">Код рассылки (например <c>"ReportReady"</c>).</param>
/// <param name="IsEnabled">Признак включения.</param>
/// <param name="Subject">Тема письма.</param>
/// <param name="Body">Тело письма (HTML/Plain).</param>
public record MailingDto(string MailingCode, bool IsEnabled, string Subject, string Body);

/// <summary>
/// Список тем по направлениям
/// </summary>
/// <param name="Name">Имя темы.</param>
/// <param name="Description">Краткое описание темы.</param>
/// <param name="DirectionId">Идентификатор направления.</param>
public record TopicDto(int Id, string Name, string Description, int DirectionId);