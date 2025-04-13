using idcc.Models;
using idcc.Models.Profile;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace idcc.Context;

public class IdccContext : IdentityDbContext<ApplicationUser>
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
    }
    
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
    /// Контекст для справочника "Роли"
    /// </summary>
    public DbSet<Role> Roles { get; set; }
    
    /// <summary>
    /// Контекст для справочника "Темы"
    /// </summary>
    public DbSet<Topic> Topics { get; set; }
    
    /// <summary>
    /// Контекст для таблицы "Вопросы"
    /// </summary>
    public DbSet<Question> Questions { get; set; }

    /// <summary>
    /// Контекст для таблицы "Ответы"
    /// </summary>
    public DbSet<Answer> Answers { get; set; }
    
    /// <summary>
    /// Контекст для таблицы "Сессия"
    /// </summary>
    public DbSet<Session> Sessions { get; set; }
    
    /// <summary>
    /// Контекст для справочника "Грейды"
    /// </summary>
    public DbSet<Grade> Grades { get; set; }
    
    /// <summary>
    /// Контекст для справочника "Веса грейдов"
    /// </summary>
    public DbSet<Weight> Weights { get; set; }
    
    /// <summary>
    /// Контекст для справочника "Баллы грейдов"
    /// </summary>
    public DbSet<GradeLevel> GradeLevels { get; set; }
    
    /// <summary>
    /// Контекст для справочника "Время ответов"
    /// </summary>
    public DbSet<AnswerTime> AnswerTimes { get; set; }
    
    /// <summary>
    /// Контекст для таблицы "Ответы пользователя"
    /// </summary>
    public DbSet<UserAnswer> UserAnswers { get; set; }
    
    /// <summary>
    /// Контекст для таблицы "Темы для пользователя"
    /// </summary>
    public DbSet<UserTopic> UserTopics { get; set; }
    
    /// <summary>
    /// Контекст для таблицы "Связи грейдов"
    /// </summary>
    public DbSet<GradeRelation> GradeRelations { get; set; }

    /// <summary>
    /// Контекст для таблицы "Проценты"
    /// </summary>
    public DbSet<Persent> Persents { get; set; }
    
    /// <summary>
    /// Контекст для таблицы "Количества"
    /// </summary>
    public DbSet<Count> Counts { get; set; }
}