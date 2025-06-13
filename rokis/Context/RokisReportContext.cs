using rokis.Models;
using rokis.Models.Config;
using rokis.Models.Mail;
using Microsoft.EntityFrameworkCore;

namespace rokis.Context;

public partial class RokisContext
{
    /// <summary>
    /// Контекст для работы с отчетами
    /// </summary>
    public DbSet<Report> Reports { get; set; }
}