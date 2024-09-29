namespace idcc.Models.Dto;

/// <summary>
/// Итоговый отчет.
/// </summary>
public record ReportDto
{
    /// <summary>
    /// Имя кандидата.
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Дата начала теста.
    /// </summary>
    public DateTime StartSession { get; set; }
    
    /// <summary>
    /// Дата завершения теста.
    /// </summary>
    public DateTime EndSession { get; set; }
    
    /// <summary>
    /// Общая продолжительность тестирования.
    /// </summary>
    public TimeSpan TestingTime { get; set; }
    
    /// <summary>
    /// Итоговая оценка.
    /// </summary>
    public FinalScoreDto? FinalScoreDto { get; set; }
    
    /// <summary>
    /// Итог по темам.
    /// </summary>
    public List<FinalTopicData>? FinalTopicDatas { get; set; }
}

/// <summary>
/// Итоговая оценка.
/// </summary>
public record FinalScoreDto
{
    /// <summary>
    /// Общий итоговый балл.
    /// </summary>
    public double Score { get; set; }
    
    /// <summary>
    /// Интерпритация уровня.
    /// </summary>
    public string Grade { get; set; }
}

/// <summary>
/// Итог по темам.
/// </summary>
public record FinalTopicData
{
    /// <summary>
    /// Название темы.
    /// </summary>
    public string Topic { get; set; }
    
    /// <summary>
    /// Общий балл по теме.
    /// </summary>
    public double Score { get; set; }
    
    /// <summary>
    /// Количество правильных вопросов.
    /// </summary>
    public int Positive { get; set; }
    
    /// <summary>
    /// Количество неправильных вопросов.
    /// </summary>
    public int Negative { get; set; }
}