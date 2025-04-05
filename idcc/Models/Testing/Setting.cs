using System.ComponentModel.DataAnnotations;

namespace idcc.Models;

public class Setting
{
    [Key]
    public int Id { get; set; }
    
    public string Code { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public int Value { get; set; }
}