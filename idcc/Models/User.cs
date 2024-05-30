using System.ComponentModel.DataAnnotations;

namespace idcc.Models;

public class User
{
    [Key]
    public int Id { get; set; }
    public string FullName { get; set; }
    public virtual Role Role { get; set; }
}