using System.ComponentModel.DataAnnotations;

namespace idcc.Models;

public class Session
{
    [Key]
    public int Id { get; set; }
    
    public virtual User User { get; set; }
    
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public double Score { get; set; }
}