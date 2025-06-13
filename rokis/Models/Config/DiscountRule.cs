using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace rokis.Models.Config;

public class DiscountRule
{
    [Key]
    public int Id { get; set; }

    /// <summary>Минимальное кол-во токенов в заказе, с которого действует скидка</summary>
    public int MinQuantity { get; set; }

    /// <summary>
    /// Максимальное кол-во (включительно).  
    /// null – «до бесконечности».
    /// </summary>
    public int? MaxQuantity { get; set; }

    /// <summary>Ставка скидки (0.1 = ‑10 %)</summary>
    [Column(TypeName = "decimal(5,4)")]
    public decimal DiscountRate { get; set; }
}