using System.ComponentModel.DataAnnotations;

namespace idcc.Models;

public class UserGrade
{
    [Key]
    public int Id { get; set; }
    public virtual User User { get; set; }
    public virtual Topic Topic { get; set; }
    public double Score { get; set; }
    public virtual Grade Current { get; set; }
    public bool IsFinished { get; set; }
}