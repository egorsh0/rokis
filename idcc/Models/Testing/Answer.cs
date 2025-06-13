using System.ComponentModel.DataAnnotations;

namespace idcc.Models;

public class Answer
{
    [Key]
    public int Id { get; set; }
    public virtual Question Question { get; set; }
    public string Content { get; set; }
    public bool IsCorrect { get; set; }
}