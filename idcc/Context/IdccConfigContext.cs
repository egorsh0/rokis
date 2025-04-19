using idcc.Models;
using idcc.Models.Config;
using Microsoft.EntityFrameworkCore;

namespace idcc.Context;

public partial class IdccContext
{
    /// <summary>
    /// Контекст для справочника "Направление"
    /// </summary>
    public DbSet<Direction> Directions { get; set; }
    
    /// <summary>
    /// Контекст для справочника "Скидки"
    /// </summary>
    public DbSet<DiscountRule> DiscountRules { get; set; }
    
    /// <summary>
    /// Контекст для справочника "Темы"
    /// </summary>
    public DbSet<Topic> Topics { get; set; }
    
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
    /// Контекст для таблицы "Связи грейдов"
    /// </summary>
    public DbSet<GradeRelation> GradeRelations { get; set; }

    /// <summary>
    /// Контекст для таблицы "Проценты"
    /// </summary>
    public DbSet<Persent> Persents { get; set; }
    
    /// <summary>
    /// Контекст для таблицы "Количества вопросов для ..."
    /// </summary>
    public DbSet<Count> Counts { get; set; }
}