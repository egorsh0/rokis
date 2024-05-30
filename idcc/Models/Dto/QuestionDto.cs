namespace idcc.Models.Dto;

public record QuestionDto
{
    public string Question { get; set; }
    public List<AnswerDto> Answers { get; set; } = new();
}

public record AnswerDto
{
    public string Answer { get; set; }
    public double Score { get; set; }
}