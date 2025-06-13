using System.Text.Json.Serialization;

namespace rokis.Dtos;

public record QuestionSmartDto(int QuestionId, bool IsMultipleChoice, double Weight);

public record QuestionShortDto()
{
    public int Id { get; set; }
    public List<AnswerShortDto> Answers { get; set; } = new();
}

public record AnswerShortDto
{
    public int Id { get; set; }
}

public record QuestionDto : QuestionShortDto
{
    public required string Topic { get; set; }
    public required string Content { get; set; }
    public new List<AnswerDto> Answers { get; set; } = new();
}

public record AnswerDto : AnswerShortDto
{
    public bool IsCorrect { get; set; }
    public string Content { get; set; }
}

public record QuestionResultDto(double Difficulty, bool IsCorrect, double TimeSeconds);