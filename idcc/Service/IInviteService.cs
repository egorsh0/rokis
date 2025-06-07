using idcc.Context;
using idcc.Models.Mail;
using Microsoft.EntityFrameworkCore;

namespace idcc.Service;

public interface IInviteService
{
    Task<Invite> CreateInviteAsync(string email);
}

public class InviteService : IInviteService
{
    private readonly IdccContext _idccContext;
    private readonly IEmailService _emailService;

    public InviteService(IdccContext idccContext, IEmailService emailService)
    {
        _idccContext = idccContext;
        _emailService = emailService;
    }

    public async Task<Invite> CreateInviteAsync(string email)
    {
        var invite = new Invite { Email = email };
        _idccContext.Invites.Add(invite);
        await _idccContext.SaveChangesAsync();

        var mailingSetting = await _idccContext.MailingSettings
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
