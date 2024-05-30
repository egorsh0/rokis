using System.ComponentModel.DataAnnotations;

namespace idcc.Models;

public class RlQuestion
{
    [Key]
    public int Id { get; set; }
    public virtual Question Question { get; set; }
    public virtual Answer Answer { get; set; }
}