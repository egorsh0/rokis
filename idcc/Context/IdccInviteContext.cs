using idcc.Models;
using idcc.Models.Config;
using idcc.Models.Mail;
using Microsoft.EntityFrameworkCore;

namespace idcc.Context;

public partial class IdccContext
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