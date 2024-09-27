namespace idcc.Infrastructures;

public static class ErrorMessage
{
    public static string ACTUAL_TOPIC_IS_NULL = "Тестирование не запущено. Открытая тема не найдена.";

    public static Func<string, string> GRADE_TIMES_IS_NULL { get; } =
        (grade) => $"В базе отсутствует конфигурация для средних значений времени  грейда {grade}.";
    
    public static Func<string, string> GRADE_WEIGHT_IS_NULL { get; } =
        (grade) => $"В базе отсутствует конфигурация для средних значений весов грейда {grade}.";
    
    public static string QUESTION_IS_NULL = "Вопрос не был найден.";
    
    public static string SESSION_IS_FINISHED = "Сессия завершена.";
    public static string TOPIC_IS_NOT_EXIST = "Сессия завершена.";
}