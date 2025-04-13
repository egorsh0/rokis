using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace idcc.Models.Profile;

public class UserProfile
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Ссылка на саму запись в AspNetUsers (UserId)
    /// </summary>
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser? User { get; set; }

    // ---------------------------
    // Поля, общие для сотрудника и физ. лица:
    // ---------------------------
    public string? Name { get; set; }

    // ---------------------------
    // Опциональная ссылка на компанию, если это сотрудник
    // ---------------------------
    public string? CompanyUserId { get; set; }  // Nullable
    // Если пользователь — сотрудник, то здесь ссылка на 
    // ApplicationUser с ролью "Company".
    // Если пользователь — физ. лицо (Person), то = null.

    // Можно сделать навигационное свойство:
    [ForeignKey(nameof(CompanyUserId))]
    public virtual ApplicationUser? CompanyUser { get; set; }
}