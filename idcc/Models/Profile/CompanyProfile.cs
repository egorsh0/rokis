using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace idcc.Models.Profile;

/// <summary>
/// Профиль (таблица) для компании.
/// Пароль хранится в Identity (ApplicationUser).
/// </summary>
public class CompanyProfile
{
    [Key]
    public int Id { get; set; }

    /// <summary>Название компании</summary>
    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>ИНН</summary>
    [Required]
    [MaxLength(12)]
    public string INN { get; set; } = string.Empty;

    /// <summary>Email компании (дублируется, но можно хранить и в Identity)</summary>
    [Required]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Ссылка на AspNetUsers.Id – 
    /// т.е. этот профиль принадлежит пользователю c ролью "Company"
    /// </summary>
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = string.Empty;

    public virtual ApplicationUser? User { get; set; }

    /// <summary>
    /// Список сотрудников (Employees), привязанных к этой компании
    /// </summary>
    public virtual List<EmployeeProfile> Employees { get; set; } = new();
}