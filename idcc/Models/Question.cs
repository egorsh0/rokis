using System.ComponentModel.DataAnnotations;

namespace idcc.Models;

public class Question
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public virtual Topic Topic { get; set; }
    public double Rank { get; set; }
}