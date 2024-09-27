using idcc.Application;
using idcc.Application.Interfaces;
using idcc.Context;
using idcc.Infrastructures;
using idcc.Infrastructures.Interfaces;
using idcc.Repository;
using idcc.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace idcc.Configurations;

public static class Configuration
{
    public static void RegisterServices(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("IdccDb");
        builder.Services.AddDbContext<IdccContext>(options =>
        {
            options.UseLazyLoadingProxies();
            options.UseNpgsql(connectionString);
        });

        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
        builder.Services.AddScoped<IUserTopicRepository, UserTopicRepository>();
        builder.Services.AddScoped<ISessionRepository, SessionRepository>();
        builder.Services.AddScoped<IUserAnswerRepository, UserAnswerRepository>();
        builder.Services.AddScoped<IDataRepository, DataRepository>();

        builder.Services.AddScoped<ITimeCalculate, TimeCalculate>();
        builder.Services.AddScoped<IWeightCalculate, WeightCalculate>();
        builder.Services.AddScoped<IScoreCalculate, ScoreCalculate>();
        builder.Services.AddScoped<IGradeCalculate, GradeCalculate>();

        builder.Services.AddScoped<IIdccApplication, IdccApplication>();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo {
                Title = "IDCC API",
                Description = "IDCC",
                Version = "v1" });
        });
    }

    public static void RegisterMiddlewares(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "IDCC API V1");
            });
        }

        app.UseHttpsRedirection();
    }
}