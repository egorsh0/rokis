using idcc.Infrastructures;
using MailKit.Net.Smtp;
using MimeKit;

namespace idcc.Service;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody);
}

public class MailKitEmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<MailKitEmailService> _log;

    public MailKitEmailService(SmtpSettings smtpSettings, ILogger<MailKitEmailService> log)
    {
        _smtpSettings = smtpSettings;
        _log = log;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        if (!await IsSmtpServerAvailable())
        {
            _log.LogError("SMTP недоступен. Письмо не отправляется.");
            return;
        }
        
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtpSettings.Username, _smtpSettings.FromEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, _smtpSettings.UseSsl);
            await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            _log.LogInformation("Password-reset mail sent to {Email}", to);
        }
        catch (Exception ex)
        {
            _log.LogError($"Ошибка при отправке письма: {ex.Message}");
        }
    }

    
    private async Task<bool> IsSmtpServerAvailable()
    {
        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, _smtpSettings.UseSsl);
            await client.DisconnectAsync(true);
            return true;
        }
        catch (Exception ex)
        {
            _log.LogError($"Ошибка при подключении к smtp: {ex.Message}");
            return false;
        }
    }
}