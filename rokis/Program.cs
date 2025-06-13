using rokis.Configurations;
using rokis.Endpoints;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

DotNetEnv.Env.Load("/app/secrets/secrets.env");

var builder = WebApplication.CreateBuilder(args);

builder.RegisterServices();

var connectionString = "http://loki:3100";
var lokiConnection = Environment.GetEnvironmentVariable("LOKI");
if (!string.IsNullOrWhiteSpace(lokiConnection))
{
    connectionString = lokiConnection;
}
builder.Host.UseSerilog((_, lc) =>
{
    lc
        .MinimumLevel.Debug()
        .WriteTo.Console()
        .WriteTo.GrafanaLoki(connectionString, labels:
        [
            new LokiLabel { Key = "app", Value = "rokis" },
            new LokiLabel { Key = "env", Value = "production" }
        ]);
});

var app = builder.Build();
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

app.RegisterMiddlewares();

app.RegisterPingEndpoints();

app.Run();