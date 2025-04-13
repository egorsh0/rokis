using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace idcc.Models.Profile;

/// <summary>
/// Профиль для "физического лица" (Person).
/// Пароль хранится в Identity.
/// </summary>
public class PersonProfile
{
    [Key]
    public int Id { get; set; }

    /// <summary>Полное имя</summary>
    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>Email (дублируется, можно хранить только в Identity)</summary>
    [Required]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = string.Empty;

    public virtual ApplicationUser? User { get; set; }
}