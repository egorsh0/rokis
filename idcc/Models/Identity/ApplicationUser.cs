using Microsoft.AspNetCore.Identity;

namespace idcc.Models;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; } = string.Empty;
}