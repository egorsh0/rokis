using idcc.Models;
using Microsoft.EntityFrameworkCore;

namespace idcc.Context;

public class IdccContext : DbContext
{
    public IdccContext(DbContextOptions<IdccContext> options) :
        base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseSerialColumns();
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<Grade> Grades { get; set; }
    public DbSet<RlGrade> RlGrades { get; set; }
    
    public DbSet<Question> Questions { get; set; }
    public DbSet<RlQuestion> RlQuestions { get; set; }
    
    public DbSet<Answer> Answers { get; set; }
    public DbSet<UserGrade> UserGrades { get; set; }
}