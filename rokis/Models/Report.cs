namespace rokis.Models;

public class Report
{
    public int Id { get; set; }

    public Guid TokenId { get; set; }
    public virtual Token Token { get; set; } = null!;

    public double Score { get; set; }
    
    public int GradeId { get; set; }
    public virtual Grade Grade { get; set; } = null!;

    public byte[]? Image { get; set; }
}
