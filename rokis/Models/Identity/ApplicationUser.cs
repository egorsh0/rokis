using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace rokis.Models;

public class ApplicationUser : IdentityUser
{
    [MaxLength(255)]
    public string DisplayName  { get; set; } = string.Empty;
    
    public DateTimeOffset PasswordLastChanged { get; set; } = DateTime.UtcNow;
}