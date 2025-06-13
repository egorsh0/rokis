using System.ComponentModel.DataAnnotations;

namespace idcc.Models;

public class Topic
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public virtual Direction Direction { get; set; }
}