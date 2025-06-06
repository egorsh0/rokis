using System.ComponentModel.DataAnnotations;

namespace idcc.Models;

public class GradeLevel
{
    [Key]
    public int Id { get; set; }
    
    public virtual required Grade Grade { get; set; }
    
    public double Min { get; set; }
    public double Max { get; set; }
}