using System.ComponentModel.DataAnnotations;

namespace idcc.Models;

public class Question
{
    [Key]
    public int Id { get; set; }
    public string Content { get; set; }
    public virtual Topic Topic { get; set; }
    public double Weight { get; set; }
    public bool IsMultipleChoice { get; set; }
}