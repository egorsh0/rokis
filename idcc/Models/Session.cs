using System.ComponentModel.DataAnnotations;

namespace idcc.Models;

public class Session
{
    [Key]
    public int Id { get; set; }
    
    public Guid TokenId { get; set; }
    public virtual Token Token { get; set; } = null!;

    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public double Score { get; set; }

    public string? EmployeeUserId { get; set; }
    public virtual ApplicationUser? Employee { get; set; }

    public string? PersonUserId { get; set; }
    public virtual ApplicationUser? Person { get; set; }
}