using System.Text.Json.Serialization;
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
        builder.Services.AddOptions();
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        var connectionString = builder.Configuration.GetConnectionString("idccDb");
        builder.Services.AddDbContext<IdccContext>(options =>
        {
            options.UseLazyLoadingProxies();
            options.UseNpgsql(connectionString);
        });

        builder.Services.AddScoped<ITokenRepository, TokenRepository>();

        builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
        builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
        builder.Services.AddScoped<IUserTopicRepository, UserTopicRepository>();
        builder.Services.AddScoped<ISessionRepository, SessionRepository>();
        builder.Services.AddScoped<IUserAnswerRepository, UserAnswerRepository>();
        builder.Services.AddScoped<IDataRepository, DataRepository>();
        builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();

        builder.Services.AddScoped<ITimeCalculate, TimeCalculate>();
        builder.Services.AddScoped<IWeightCalculate, WeightCalculate>();
        builder.Services.AddScoped<IScoreCalculate, ScoreCalculate>();
        builder.Services.AddScoped<IGradeCalculate, GradeCalculate>();
        
        builder.Services.AddScoped<IGraphGenerate, GraphGenerate>();

        builder.Services.AddScoped<IIdccApplication, IdccApplication>();
        builder.Services.AddScoped<IIdccReport, IdccReport>();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo {
                Title = "MVP Аттестация API",
                Description = "MVP API",
                Version = "v1" });
        });
    }

    public static void RegisterMiddlewares(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "MVP Аттестация API");
        });

        app.UseHttpsRedirection();
        app.UseAuthorization();
    }
}