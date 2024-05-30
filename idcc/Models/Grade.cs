using System.ComponentModel.DataAnnotations;

namespace idcc.Models;

public class Grade
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public double Score { get; set; }
}