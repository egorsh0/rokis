using System.ComponentModel.DataAnnotations;

namespace idcc.Models;

public class Weight
{
    [Key]
    public int Id { get; set; }
    
    public virtual Grade Grade { get; set; }
    
    public double Min { get; set; }
    
    public double Max { get; set; }
}