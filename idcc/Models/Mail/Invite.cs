namespace idcc.Models.Mail;

public class Invite
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; }
    public string InviteCode { get; set; } = Guid.NewGuid().ToString();
    public bool IsUsed { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}