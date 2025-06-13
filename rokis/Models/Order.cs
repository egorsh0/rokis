using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using rokis.Infrastructures;

namespace rokis.Models;

public class Order
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;             // AspNetUsers.Id
    public string Role { get; set; } = null!;               // "Company" или "Person"

    public int Quantity { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountRate { get; set; }               // например, 0.1m = 10%
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountedTotal { get; set; }
    
    public OrderStatus Status   { get; set; } = OrderStatus.Unpaid;
    public DateTime?   PaidAt   { get; set; }
    
    /// <summary>Id транзакции/чека в платёжном шлюзе.</summary>
    [MaxLength(64)]
    public string?      PaymentId   { get; set; }

    public virtual ICollection<Token> Tokens { get; set; } = new List<Token>();
}