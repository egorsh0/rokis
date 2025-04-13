using idcc.Models;

namespace idcc.Dtos;

public record QuestionShortDto
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
    public string Topic { get; set; }
    public string Content { get; set; }
    public List<AnswerDto> Answers { get; set; } = new();
}

public record AnswerDto : AnswerShortDto
{
    public string Content { get; set; }
}

public record QuestionDataDto
{
    public Question Question;
    public List<Answer> Answers { get; set; } = new();
}