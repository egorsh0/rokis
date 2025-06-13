namespace idcc.Infrastructures;

public static class ErrorMessages
{
    public static Func<string, string> GRADE_TIMES_IS_NULL { get; } =
        (grade) => $"В базе отсутствует конфигурация для средних значений времени грейда {grade}.";
    
    public static Func<string, string> GRADE_WEIGHT_IS_NULL { get; } =
        (grade) => $"В базе отсутствует конфигурация для средних значений весов грейда {grade}.";
    
    public static Func<string, string> GRADE_RELATIONS_IS_NULL { get; } =
        (grade) => $"В базе отсутствует конфигурация для связей грейда {grade}.";
}