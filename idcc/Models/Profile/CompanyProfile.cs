using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace idcc.Models.Profile;

public class CompanyProfile
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Название организации (можно использовать вместо Name в ApplicationUser)
    /// </summary>
    public string OrganizationName { get; set; } = string.Empty;

    /// <summary>
    /// ИНН
    /// </summary>
    public string INN { get; set; } = string.Empty;

    /// <summary>
    /// Связь 1:1 с пользователем, у которого роль = "Company"
    /// </summary>
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = string.Empty;

    public virtual ApplicationUser? User { get; set; }
}