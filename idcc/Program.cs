using idcc.Context;
using idcc.Models;
using idcc.Models.Dto;
using idcc.Repository;
using idcc.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Grade = idcc.Infrastructures.Grade;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("IdccDb");
builder.Services.AddDbContext<IdccContext>(options =>
{
    options.UseLazyLoadingProxies();
    options.UseNpgsql(connectionString);
});

builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();

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

app.MapPost("/user", async (UserDto userDto, IRoleRepository roleRepository, IUserRepository userRepository) =>
{
    var role = await roleRepository.GetRoleAsync(userDto.Role.Name);
    if (role is null)
    {
        return Results.BadRequest(string.Format("Role with name {name} not found", userDto.Role.Name));
    }

    var user = new User()
    {
        Role = role,
        FullName = userDto.FullName
    };
    
    var resultUser = await userRepository.CreateAsync(user);
    return Results.Ok(resultUser);
});

app.MapGet("/question", async (int userId, IQuestionRepository questionRepository) =>
{
    
});

app.MapPatch("/updateScore", () => { });

app.Run();