using rokis.Configurations;
using rokis.Endpoints;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

DotNetEnv.Env.Load("/app/secrets/secrets.env");

var builder = WebApplication.CreateBuilder(args);

builder.RegisterServices();

var lokiConnection = builder.Configuration.GetConnectionString("loki") ?? "http://loki:3100";
builder.Host.UseSerilog((_, lc) =>
{
    lc
        .MinimumLevel.Debug()
        .WriteTo.Console()
        .WriteTo.GrafanaLoki(lokiConnection, labels:
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