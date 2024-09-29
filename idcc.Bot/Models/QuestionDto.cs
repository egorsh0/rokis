namespace idcc.Bot.Models;

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
    public string Topic { get; set; } = default!;
    public string Content { get; set; } = default!;
    public new List<AnswerDto> Answers { get; set; } = new();
}

public record AnswerDto : AnswerShortDto
{
    public string Content { get; set; } = default!;
}