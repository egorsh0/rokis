using System.ComponentModel.DataAnnotations.Schema;
using rokis.Infrastructures;

namespace rokis.Models;

public class Token
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public int DirectionId { get; set; }
    public virtual Direction Direction { get; set; } = null!;

    public TokenStatus Status { get; set; } = TokenStatus.Pending;

    /// <summary>Цена за единицу</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; } // копируем цену из Direction.BasePrice

    public DateTime PurchaseDate { get; set; }
    
    /// <summary>Ссылка на заказ</summary>
    public int? OrderId { get; set; }
    public virtual Order? Order { get; set; }

    /// <summary>Если привязан к сотруднику</summary>
    public string? EmployeeUserId { get; set; }
    public virtual ApplicationUser? Employee { get; set; }

    /// <summary>Если привязан к физ. лицу</summary>
    public string? PersonUserId { get; set; }
    public virtual ApplicationUser? Person { get; set; }

    /// <summary>Балл, полученный по токену (null — ещё не проходил).</summary>
    public double? Score { get; set; }
    
    /// <summary>URL сертификата, если сессия завершена и отчёт сгенерирован.</summary>
    public string? CertificateUrl { get; set; }
}