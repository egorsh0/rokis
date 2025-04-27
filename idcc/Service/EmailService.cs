using idcc.Infrastructures;
using MailKit.Net.Smtp;
using MimeKit;

namespace idcc.Service;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
}

public class MailKitEmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;

    public MailKitEmailService(SmtpSettings smtpSettings)
    {
        _smtpSettings = smtpSettings;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        if (!await IsSmtpServerAvailable())
        {
            Console.WriteLine("SMTP недоступен. Письмо не отправляется.");
            return;
        }
        
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtpSettings.FromName, _smtpSettings.FromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("plain")
        {
            Text = body
        };

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, _smtpSettings.EnableSsl);
            await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при отправке письма: {ex.Message}");
        }
    }

    
    private async Task<bool> IsSmtpServerAvailable()
    {
        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, _smtpSettings.EnableSsl);
            await client.DisconnectAsync(true);
            return true;
        }
        catch
        {
            return false;
        }
    }
}