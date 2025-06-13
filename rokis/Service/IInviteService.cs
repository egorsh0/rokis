using rokis.Context;
using rokis.Models.Mail;
using Microsoft.EntityFrameworkCore;

namespace rokis.Service;

public interface IInviteService
{
    Task<Invite> CreateInviteAsync(string email);
}

public class InviteService : IInviteService
{
    private readonly RokisContext _rokisContext;
    private readonly IEmailService _emailService;

    public InviteService(RokisContext rokisContext, IEmailService emailService)
    {
        _rokisContext = rokisContext;
        _emailService = emailService;
    }

    public async Task<Invite> CreateInviteAsync(string email)
    {
        var invite = new Invite { Email = email };
        _rokisContext.Invites.Add(invite);
        await _rokisContext.SaveChangesAsync();

        var mailingSetting = await _rokisContext.MailingSettings
            .FirstOrDefaultAsync(x => x.MailingCode == "Invite");

        if (mailingSetting?.IsEnabled != true)
        {
            return invite;
        }
        var subject = mailingSetting.Subject;
        var body = mailingSetting.Body.Replace("{InviteCode}", invite.InviteCode);

        await _emailService.SendEmailAsync(email, subject, body);

        return invite;
    }
}
