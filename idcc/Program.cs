using idcc.Application;
using idcc.Application.Interfaces;
using idcc.Context;
using idcc.Infrastructures;
using idcc.Infrastructures.Interfaces;
using idcc.Models;
using idcc.Models.Dto;
using idcc.Repository;
using idcc.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "IDCC API V1");
    });
}

app.MapPost("/user", async (UserDto userDto, IUserRepository userRepository) =>
{
    var role = await userRepository.GetRoleAsync(userDto.Role.Name);
    if (role is null)
    {
        return Results.BadRequest(string.Format("Role with name {name} not found", userDto.Role.Name));
    }

    var user = new User()
    {
        Role = role,
        UserName = userDto.UserName,
        PasswordHash = userDto.PasswordHash,
        RegistrationDate = DateTime.Now
    };
    
    var resultUser = await userRepository.CreateAsync(user);
    return Results.Ok(resultUser);
});

app.MapPost("/session/start", async (int userId, ISessionRepository sessionRepository, IUserRepository userRepository) =>
{
    var user = await userRepository.GetUserAsync(userId);
    if (user is null)
    {
        return Results.NotFound();
    }

    var session = await sessionRepository.GetSessionAsync(userId);
    if (session is not null)
    {
        return Results.BadRequest();
    }
    await sessionRepository.StartSessionAsync(user);
    return Results.Ok();

});

app.MapPost("/session/stop", async (int userId, ISessionRepository sessionRepository, IUserRepository userRepository) =>
{
    var user = await userRepository.GetUserAsync(userId);
    if (user is null)
    {
        return Results.NotFound();
    }

    var session = await sessionRepository.GetSessionAsync(userId);
    if (session is null)
    {
        return Results.BadRequest();
    }

    if (session.EndTime is not null)
    {
        return Results.BadRequest();
    }
    await sessionRepository.EndSessionAsync(user);
    return Results.Ok();
});

app.MapGet("/question/{userId}", async (int userId, IUserTopicRepository userTopicRepository, IQuestionRepository questionRepository) =>
{
    // Проверка на открытые темы

    var hasOpenTopics = await userTopicRepository.HasOpenTopic(userId);
    if (hasOpenTopics)
    {
        // Получение рандомной темы
        var userTopic = await userTopicRepository.GetRandomTopicAsync(userId);
        if (userTopic is null)
        {
            return Results.NoContent();
        }
        
        var question = await questionRepository.GetQuestionAsync(userTopic);
        if (question is null)
        {
            return Results.BadRequest();
        }
        
        await userTopicRepository.UpdateTopicInfoAsync(userTopic.Id, true, true);
        Results.Ok(question);
    }
    
    return Results.BadRequest();
});

app.MapPost("/question/sendAnswers", async (int userId, TimeSpan dateInterval, QuestionDto question,
    IUserRepository userRepository,
    ISessionRepository sessionRepository,
    IIdccApplication idccApplication
) =>
{
    var session = await sessionRepository.GetSessionAsync(userId);
    if (session is null)
    {
        return Results.NotFound();
    }
    // Посчитать и сохранить Score за ответ
    var result = await idccApplication.CalculateScoreAsync(session, userId, dateInterval.Seconds, question.Id,
        question.Answers.Select(_ => _.Id));
    if (result is null)
    {
        return Results.NotFound(result);
    }
    // Пересчитать вес текущего топика
    result = await idccApplication.CalculateTopicWeightAsync(session, userId);
    return result is not null ? Results.NotFound(result) : Results.Ok();
});

app.Run();