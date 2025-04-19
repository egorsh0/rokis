using idcc.Models;
using idcc.Models.Config;
using idcc.Models.Profile;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace idcc.Context;

public partial class IdccContext : IdentityDbContext<ApplicationUser>
{
    /// <inheritdoc />
    public IdccContext(DbContextOptions<IdccContext> options) :
        base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.UseSerialColumns();
        
        // CompanyProfile -> AspNetUsers
        builder.Entity<CompanyProfile>(entity =>
        {
            entity.HasOne(cp => cp.User)
                .WithOne() 
                .HasForeignKey<CompanyProfile>(cp => cp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Связь 1 (CompanyProfile) -> многие (EmployeeProfile)
            // Но мы настроим её в EmployeeProfile:
            // entity.HasMany(cp => cp.Employees)
            //       .WithOne(e => e.Company)
            //       .HasForeignKey(e => e.CompanyProfileId);
        });

        // EmployeeProfile -> AspNetUsers
        builder.Entity<EmployeeProfile>(entity =>
        {
            entity.HasOne(ep => ep.User)
                .WithOne()
                .HasForeignKey<EmployeeProfile>(ep => ep.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Связь EmployeeProfile -> CompanyProfile
            entity.HasOne(ep => ep.Company)
                .WithMany(cp => cp.Employees)
                .HasForeignKey(ep => ep.CompanyProfileId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // PersonProfile -> AspNetUsers
        builder.Entity<PersonProfile>(entity =>
        {
            entity.HasOne(pp => pp.User)
                .WithOne()
                .HasForeignKey<PersonProfile>(pp => pp.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        builder.Entity<Direction>().HasIndex(d => d.Name).IsUnique();
        builder.Entity<DiscountRule>()
            .HasIndex(r => r.MinQuantity)
            .IsUnique(false);
        builder.Entity<Token>().HasKey(t => t.Id);
        builder.Entity<Order>().HasMany(o => o.Tokens).WithOne(t => t.Order).HasForeignKey(t => t.OrderId);
        builder.Entity<Session>()
            .HasOne(s => s.Token).WithMany()
            .HasForeignKey(s => s.TokenId);
    }
    
    /// <summary>
    /// Контекст для таблицы "Администратор"
    /// </summary>
    public DbSet<AdministratorProfile> AdministratorProfiles { get; set; } = null!;
    
    /// <summary>
    /// Контекст для таблицы "Компания"
    /// </summary>
    public DbSet<CompanyProfile> CompanyProfiles { get; set; } = null!;
    
    /// <summary>
    /// Контекст для таблицы "Сотрудник"
    /// </summary>
    public DbSet<EmployeeProfile> EmployeeProfiles { get; set; } = null!;
    
    /// <summary>
    /// Контекст для таблицы "Физ лицо"
    /// </summary>
    public DbSet<PersonProfile> PersonProfiles { get; set; } = null!;
    
    /// <summary>
    /// Контекст для таблицы "Токены"
    /// </summary>
    public DbSet<Token> Tokens { get; set; }
    
    /// <summary>
    /// Контекст для таблицы "Заказы"
    /// </summary>
    public DbSet<Order> Orders { get; set; } = null!;
    
    /// <summary>
    /// Контекст для таблицы "Сессия"
    /// </summary>
    public DbSet<Session> Sessions { get; set; }
    
    /// <summary>
    /// Контекст для таблицы "Вопросы"
    /// </summary>
    public DbSet<Question> Questions { get; set; }

    /// <summary>
    /// Контекст для таблицы "Ответы"
    /// </summary>
    public DbSet<Answer> Answers { get; set; }
    
    /// <summary>
    /// Контекст для таблицы "Ответы пользователя"
    /// </summary>
    public DbSet<UserAnswer> UserAnswers { get; set; }
    
    /// <summary>
    /// Контекст для таблицы "Темы для пользователя"
    /// </summary>
    public DbSet<UserTopic> UserTopics { get; set; }
}