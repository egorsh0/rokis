using System.ComponentModel.DataAnnotations;

namespace rokis.Models;

public class GradeRelation
{
    [Key]
    public int Id { get; set; }
    
    public virtual Grade? Start { get; set; }
    
    public virtual Grade? End { get; set; }
}