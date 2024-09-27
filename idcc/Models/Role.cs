using System.ComponentModel.DataAnnotations;

namespace idcc.Models;

public class Role
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
}