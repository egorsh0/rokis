using idcc.Configurations;
using idcc.Endpoints;
using idcc.Endpoints.AdminPanel;

var builder = WebApplication.CreateBuilder(args);

builder.RegisterServices();

var app = builder.Build();
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

app.RegisterMiddlewares();

app.RegisterUserEndpoints();
app.RegisterSessionEndpoints();
app.RegisterQuestionEndpoints();
app.RegisterReportEndpoints();
app.RegisterQuestionAdminEndpoints();
app.RegisterSettingsEndpoints();

app.Run();