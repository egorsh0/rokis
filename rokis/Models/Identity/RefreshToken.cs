using System.ComponentModel.DataAnnotations;

namespace rokis.Models;

public class RefreshToken
{
    [Key] 
    public int Id { get; set; }

    [Required]
    public string Token { get; set; } = null!;
    public DateTime Expires  { get; set; }
    public DateTime Created  { get; set; }
    public DateTime? Revoked { get; set; }

    public bool IsActive => Revoked == null && !IsExpired;
    public bool IsExpired => DateTime.UtcNow >= Expires;
    
    [Required]
    public string UserId { get; set; } = null!;
    public virtual ApplicationUser User { get; set; } = null!;
}