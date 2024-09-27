using System.ComponentModel.DataAnnotations;

namespace idcc.Models;

public class GradeLevel
{
    [Key]
    public int Id { get; set; }
    
    public virtual Grade Grade { get; set; }
    
    public double Level { get; set; }
    
}