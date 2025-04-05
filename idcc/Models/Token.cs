using System.ComponentModel.DataAnnotations;

namespace idcc.Models;

public class Token
{
    [Key]
    public int Id { get; set; }
    public string Code { get; set; }
    public string Direction { get; set; }
    public string Status { get; set; }
    public string ResultPdfUrl { get; set; }
    
    public virtual Employee Employee { get; set; }
}