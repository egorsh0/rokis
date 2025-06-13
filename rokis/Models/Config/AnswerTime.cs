using System.ComponentModel.DataAnnotations;

namespace rokis.Models;

public class AnswerTime
{
    [Key]
    public int Id { get; set; }
    
    public virtual Grade Grade { get; set; }
    
    public double Average { get; set; }
    
    public double Min { get; set; }
    
    public double Max { get; set; }
}