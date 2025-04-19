using idcc.Configurations;
using idcc.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.RegisterServices();

var app = builder.Build();
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

app.RegisterMiddlewares();

app.RegisterPingEndpoints();
app.RegisterTokenEndpoint();

app.RegisterSessionEndpoints();
app.RegisterQuestionEndpoints();
app.RegisterReportEndpoints();

app.Run();