using idcc.Configurations;
using idcc.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.RegisterServices();

var app = builder.Build();
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

app.RegisterMiddlewares();

app.RegisterPingEndpoints();

app.Run();