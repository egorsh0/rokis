using System.ComponentModel.DataAnnotations;

namespace idcc.Models;

public class UserTopic
{
    [Key]
    public int Id { get; set; }
    
    public virtual User User { get; set; }
    public virtual Topic Topic { get; set; }
    
    public virtual Grade Grade { get; set; }
    
    public double Weight { get; set; }
    
    public bool IsFinished { get; set; }
    
    public bool WasPrevious { get; set; }

    public bool Actual { get; set; }
    
    public int Count { get; set; }
}