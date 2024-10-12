using System.ComponentModel.DataAnnotations;

namespace idcc.Models.AdminDto;

public record QuestionAdminDto
{
    [Required]
    public string Content { get; set; }
    [Required]
    public string Topic { get; set; }
    [Required]
    public double Weight { get; set; }

    public bool IsMultipleChoice { get; set; } = false;
    
    [Required]
    public List<AnswerAdminDto> Answers { get; set; }
}

public record AnswerAdminDto
{
    [Required]
    public string Content { get; set; }
    [Required]
    public bool IsCorrect { get; set; }
}