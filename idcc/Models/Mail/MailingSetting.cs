namespace idcc.Models.Mail;

public class MailingSetting
{
    public int Id { get; set; }
    public required string MailingCode { get; set; } // Например: "INVITE"
    public bool IsEnabled { get; set; }
    public required string Subject { get; set; }     // Тема письма
    public required string Body { get; set; }         // Текст письма (с плейсхолдерами типа {InviteCode})
}