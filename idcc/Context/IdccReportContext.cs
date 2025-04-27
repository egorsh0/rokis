using idcc.Models;
using idcc.Models.Config;
using idcc.Models.Mail;
using Microsoft.EntityFrameworkCore;

namespace idcc.Context;

public partial class IdccContext
{
    /// <summary>
    /// Контекст для работы с отчетами
    /// </summary>
    public DbSet<Report> Reports { get; set; }
}