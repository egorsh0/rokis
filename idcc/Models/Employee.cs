using System.ComponentModel.DataAnnotations;

namespace idcc.Models;

public class Employee
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Token { get; set; }
    public virtual Company Company { get; set; }
    public virtual DateTime RegistrationDate { get; set; }
}