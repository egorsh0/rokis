using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace idcc.Models;

public class Direction
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    
    /// <summary>Базовая цена одного токена данного направления</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal BasePrice  { get; set; }

    public virtual ICollection<Token> Tokens { get; set; }
}