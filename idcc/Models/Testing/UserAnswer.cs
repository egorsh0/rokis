using System.ComponentModel.DataAnnotations;

namespace idcc.Models;

public class UserAnswer
{
    [Key]
    public int Id { get; set; }
    
    public virtual Session Session { get; set; }
    
    public virtual Question Question { get; set; }

    public double TimeSpent { get; set; }
    
    public double Score { get; set; }
    
    public DateTime AnswerTime { get; set; }
}