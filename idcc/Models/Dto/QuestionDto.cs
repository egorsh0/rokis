namespace idcc.Models.Dto;

public record QuestionDto
{
    public int Id { get; set; }
    public Topic Topic { get; set; }
    public string Content { get; set; }
    public List<AnswerDto> Answers { get; set; } = new();
}

public record AnswerDto
{
    public int Id { get; set; }
    public string Content { get; set; }
}

public record QuestionDataDto
{
    public Question Question;
    public List<Answer> Answers { get; set; } = new();
}