using System.ComponentModel.DataAnnotations.Schema;

namespace idcc.Models;

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

    public virtual ICollection<Token> Tokens { get; set; } = new List<Token>();
}