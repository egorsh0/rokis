namespace idcc.Infrastructures;

public static class ErrorMessages
{
    public static string ACTUAL_TOPIC_IS_NULL = "Тестирование не запущено. Открытая тема не найдена.";

    public static Func<string, string> GRADE_TIMES_IS_NULL { get; } =
        (grade) => $"В базе отсутствует конфигурация для средних значений времени  грейда {grade}.";
    
    public static Func<string, string> GRADE_WEIGHT_IS_NULL { get; } =
        (grade) => $"В базе отсутствует конфигурация для средних значений весов грейда {grade}.";
    
    public static string USER_IS_NULL = "Пользователь не был найден.";
    
    public static string QUESTION_IS_NULL = "Вопрос не был найден.";
    public static string QUESTIONS_IS_EMPTY = "Список вопросов пустой.";
    public static string QUESTION_IS_NOT_MULTIPLY = "Вопрос с одним вариантом ответа.";
    public static string ANSWER_ID_NOT_FOUND = "Ответ не привязан к вопросу.";
    
    public static string SESSION_IS_FINISHED = "Сессия завершена.";
    public static string SESSION_IS_NOT_FINISHED = "Сессия не завершена.";
    public static string SESSION_IS_NOT_EXIST = "Сессии не существует.";
    public static string TOPIC_IS_NOT_EXIST = "Темы не существует.";
    public static string GET_RANDOM_TOPIC = "Не получилось получить рандомную тему.";
    
    public static string REPORT_IS_FAILED = "Не получилось создать отчет.";
    public static string REPORT_ALREADY_EXISTS = "Отчет уже существует.";
    public static string REPORT_NOT_FOUND = "Отчет не найден.";
}