using System.ComponentModel.DataAnnotations.Schema;
using idcc.Infrastructures;

namespace idcc.Models;

public class Token
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public int DirectionId { get; set; }
    public virtual Direction Direction { get; set; } = null!;

    public TokenStatus Status { get; set; } = TokenStatus.Unused;

    /// <summary>Цена за единицу</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; } // копируем цену из Direction.BasePrice

    /// <summary>Ссылка на заказ</summary>
    public int? OrderId { get; set; }
    public virtual Order? Order { get; set; }

    /// <summary>Если привязан к сотруднику</summary>
    public string? EmployeeUserId { get; set; }
    public virtual ApplicationUser? Employee { get; set; }

    /// <summary>Если привязан к физ. лицу</summary>
    public string? PersonUserId { get; set; }
    public virtual ApplicationUser? Person { get; set; }

    /// <summary>Ссылка на PDF-сертификат (для использованных)</summary>
    public string? CertificateUrl { get; set; }
}