using System.ComponentModel.DataAnnotations;
using idcc.Models.Profile;

namespace idcc.Models;

public class Session
{
    [Key]
    public int Id { get; set; }
    
    // ЯВНОЕ поле внешнего ключа
    public int UserId { get; set; }
    public virtual UserProfile UserProfile { get; set; }
    
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public double Score { get; set; }
    public virtual Role Role { get; set; }
}