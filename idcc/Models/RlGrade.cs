using System.ComponentModel.DataAnnotations;

namespace idcc.Models;

public class RlGrade
{
    [Key]
    public int Id { get; set; }
    public virtual Grade CurrentGrade { get; set; }
    public virtual Grade? NextGrade { get; set; }
    public virtual Grade? PrevGrade { get; set; }
}