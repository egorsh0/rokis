using System.Text.Json.Serialization;

namespace idcc.Infrastructures;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SettingType
{
    AnswerTime,
    Count,
    GradeLevel,
    Persent,
    Weight
}