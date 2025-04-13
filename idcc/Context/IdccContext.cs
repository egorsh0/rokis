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
        
        // Настраиваем связи 1:1 с ApplicationUser:
        // CompanyProfile -> ApplicationUser
        builder.Entity<CompanyProfile>()
            .HasOne(cp => cp.User)
            .WithOne()
            .HasForeignKey<CompanyProfile>(cp => cp.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Entity<UserProfile>()
            .HasOne(up => up.User)
            .WithOne()  // или WithOne(u => u.UserProfile), если хотим обратную ссылку
            .HasForeignKey<UserProfile>(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // При желании настраиваем связь на CompanyUser:
        builder.Entity<UserProfile>()
            .HasOne(up => up.CompanyUser)
            .WithMany()  // или WithMany(c => c.Employees), если хотим список сотрудников
            .HasForeignKey(up => up.CompanyUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
    
    /// <summary>
    /// Контекст для таблицы "Компания"
    /// </summary>
    public DbSet<CompanyProfile> CompanyProfiles { get; set; } = null!;
    
    /// <summary>
    /// Контекст для таблицы "Пользователь"
    /// </summary>
    public DbSet<UserProfile> UserProfiles { get; set; } = null!;
    
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