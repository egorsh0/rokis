using idcc.Configurations;
using idcc.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.RegisterServices();

var app = builder.Build();

app.RegisterMiddlewares();

app.RegisterUserEndpoints();
app.RegisterSessionEndpoints();
app.RegisterQuestionEndpoints();

app.Run();