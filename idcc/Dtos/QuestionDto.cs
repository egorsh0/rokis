using idcc.Models;

namespace idcc.Dtos;

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
    public required string Content { get; set; }
}

public record QuestionDataDto
{
    public required Question Question;
    public List<Answer> Answers { get; set; } = new();
}

public record QuestionResultDto(double Difficulty, bool IsCorrect, double TimeSeconds);