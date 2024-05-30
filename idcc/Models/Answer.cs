using System.ComponentModel.DataAnnotations;

namespace idcc.Models;

public class Answer
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public double Score { get; set; }
}