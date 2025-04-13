using System.ComponentModel.DataAnnotations;
using idcc.Models.Profile;

namespace idcc.Models;

public class Token
{
    [Key]
    public int Id { get; set; }
    public string Code { get; set; }
    public string Direction { get; set; }
    public string Status { get; set; }
    public string ResultPdfUrl { get; set; }
    
    public virtual UserProfile UserProfile { get; set; }
}