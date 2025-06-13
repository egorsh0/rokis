using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace rokis.Models.Profile;

/// <summary>
/// Профиль для сотрудника (Employee).
/// Пароль в IdentityUser, тут – FullName, Email.
/// Привязка к компании через CompanyProfileId.
/// </summary>
public class EmployeeProfile
{
    [Key]
    public int Id { get; set; }

    /// <summary>Полное имя</summary>
    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>Email сотрудника (дублируем из Identity, если нужно)</summary>
    [Required]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Ссылка на AspNetUsers.Id – 
    /// т.е. этот профиль принадлежит пользователю c ролью "Employee"
    /// </summary>
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = string.Empty;

    public virtual ApplicationUser? User { get; set; }

    /// <summary>
    /// Если сотрудник привязан к компании, то здесь Id записи CompanyProfile
    /// </summary>
    public int? CompanyProfileId { get; set; }

    /// <summary>
    /// Навигационное свойство – к какой компании привязан
    /// </summary>
    public virtual CompanyProfile? Company { get; set; }
}