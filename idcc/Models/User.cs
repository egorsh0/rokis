using System.ComponentModel.DataAnnotations;

namespace idcc.Models;

public class User
{
    [Key]
    public int Id { get; set; }
    public string UserName { get; set; }
    public string PasswordHash { get; set; }
    public virtual Role Role { get; set; }
    
    public virtual DateTime RegistrationDate { get; set; }
}