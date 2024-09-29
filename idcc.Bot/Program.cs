/*
using idcc.Bot;
using idcc.Bot.Services;
using Microsoft.Extensions.Options;
using Telegram.Bot;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {        
        // Register Bot configuration
        services.AddOptions();
        services.Configure<AppSettings>(context.Configuration.GetSection("Configuration"));

        services.AddTransient(ser => ser.GetRequiredService<IOptions<AppSettings>>().Value);

        // Register named HttpClient to benefits from IHttpClientFactory
        // and consume it with ITelegramBotClient typed client.
        // More read:
        //  https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0#typed-clients
        //  https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
        

        services.AddHttpClient("telegram_bot_client").RemoveAllLoggers()
            .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
            {
                AppSettings? botConfiguration = sp.GetService<IOptions<AppSettings>>()?.Value;
                ArgumentNullException.ThrowIfNull(botConfiguration);
                TelegramBotClientOptions options = new(botConfiguration.BotToken);
                return new TelegramBotClient(options, httpClient);
            });

        services.AddHttpClient<IIdccService, IdccService>();
        services.AddScoped<BotService>();
        services.AddScoped<ReceiverService>();
        services.AddHostedService<PollingService>();
    })
    .Build();

await host.RunAsync();*/

using Microsoft.Extensions.Options;
using Telegram.Bot;
using idcc.Bot;
using idcc.Bot.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Register Bot configuration
        services.AddOptions();
        services.Configure<BotConfiguration>(context.Configuration.GetSection("BotConfiguration"));
        services.AddTransient(ser => ser.GetRequiredService<IOptions<BotConfiguration>>().Value);
        services.AddHttpClient<IIdccService, IdccService>();

        // Register named HttpClient to benefits from IHttpClientFactory
        // and consume it with ITelegramBotClient typed client.
        // More read:
        //  https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0#typed-clients
        //  https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
        services.AddHttpClient("telegram_bot_client").RemoveAllLoggers()
            .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
            {
                BotConfiguration? botConfiguration = sp.GetService<IOptions<BotConfiguration>>()?.Value;
                ArgumentNullException.ThrowIfNull(botConfiguration);
                TelegramBotClientOptions options = new(botConfiguration.BotToken);
                return new TelegramBotClient(options, httpClient);
            });

        services.AddScoped<UpdateHandler>();
        services.AddScoped<ReceiverService>();
        services.AddHostedService<PollingService>();
    })
    .Build();

await host.RunAsync();
