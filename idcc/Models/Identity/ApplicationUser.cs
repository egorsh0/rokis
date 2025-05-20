using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace idcc.Models;

public class ApplicationUser : IdentityUser
{
    [MaxLength(255)]
    public string DisplayName  { get; set; } = string.Empty;
    
    public DateTime PasswordLastChanged { get; set; } = DateTime.UtcNow;
}