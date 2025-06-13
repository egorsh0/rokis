using rokis.Models.Mail;
using Microsoft.EntityFrameworkCore;

namespace rokis.Context;

public partial class RokisContext
{
    /// <summary>
    /// Контекст для работы с инвайтами
    /// </summary>
    public DbSet<Invite> Invites { get; set; }
    
    /// <summary>
    /// Контекст для настроек рассылок
    /// </summary>
    public DbSet<MailingSetting> MailingSettings { get; set; }
}