using System.ComponentModel.DataAnnotations;

namespace idcc.Models.Config;

public class Time
{
    [Key]
    public int Id { get; set; }
    
    public string Code { get; set; }
    public string Description { get; set; }
    public double Value { get; set; }
}