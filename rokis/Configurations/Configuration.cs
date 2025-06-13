using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using rokis.Application.Interfaces;
using rokis.Context;
using rokis.Filters;
using rokis.Infrastructures;
using rokis.Middlewares;
using rokis.Models;
using rokis.Providers;
using rokis.Repository;
using rokis.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Prometheus;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace rokis.Configurations;

public static class Configuration
{
    public static void RegisterServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions();
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        var connectionString = builder.Configuration.GetConnectionString("rokisDb");
        
        // Подключение БД через env
        var dbConnection = Environment.GetEnvironmentVariable("DB_CONN");
        if (!string.IsNullOrWhiteSpace(dbConnection))
        {
            connectionString = dbConnection;
        }
        
        // Подключение Redis через env
        var redisConnectionString = builder.Configuration.GetConnectionString("RadisConnection");
        var redisConnection = Environment.GetEnvironmentVariable("CACHE_CONN");
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            redisConnectionString = redisConnection;
        }
        // 0. Подключаем кэширование.
        builder.Services.AddFusionCache()
            .WithDefaultEntryOptions(options => options.Duration = TimeSpan.FromMinutes(5))
            .WithSerializer(new FusionCacheSystemTextJsonSerializer())
            .WithDistributedCache(
                new RedisCache(new RedisCacheOptions
                {
                    
                    Configuration = redisConnectionString
                })
                ).AsHybridCache();
        
        // 0.0 Подключаем рассылки
        
        var smtpSettings = builder.Configuration.GetSection("Smtp").Get<SmtpSettings>();
        if (smtpSettings != null)
        {
            // Подключение Smtp через env
            var smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST");
            var smtpUser = Environment.GetEnvironmentVariable("SMTP_USER");
            var smtpPass = Environment.GetEnvironmentVariable("SMTP_PASS");
            var smtpFrom = Environment.GetEnvironmentVariable("SMTP_FROM");
            if (!string.IsNullOrWhiteSpace(smtpHost) &&
                !string.IsNullOrWhiteSpace(smtpUser) &&
                !string.IsNullOrWhiteSpace(smtpPass) &&
                !string.IsNullOrWhiteSpace(smtpFrom))
            {
                smtpSettings.Host = smtpHost;
                smtpSettings.Username = smtpUser;
                smtpSettings.Password = smtpPass;
                smtpSettings.FromEmail = smtpFrom;
            }
            
            builder.Services.AddSingleton(smtpSettings);
            builder.Services.AddScoped<IEmailService, MailKitEmailService>();
            builder.Services.AddScoped<IInviteService, InviteService>();
        }

        // 1. Подключаем EF Core (PostgreSQL)
        builder.Services.AddDbContext<RokisContext>(options =>
        {
            options.UseLazyLoadingProxies();
            options.UseNpgsql(connectionString, npg =>
            {
                npg.EnableRetryOnFailure(5);
            });
        });
        
        // 2. Настраиваем Identity (с параметрами безопасности паролей)
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Настройки защиты паролей
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8; // минимальная длина 8 символов
                options.Password.RequireNonAlphanumeric = false;

                // Политика блокировки при многократных неверных попытках входа
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); 
                options.Lockout.MaxFailedAccessAttempts = 5; 
                options.Lockout.AllowedForNewUsers = true;

                // Настройка уникальности и формат email
                options.User.RequireUniqueEmail = true; 
            })
            .AddEntityFrameworkStores<RokisContext>()
            .AddDefaultTokenProviders();

        // 3. Подключаем аутентификацию через JWT
        var jwtSecret = builder.Configuration["Jwt:Secret"];
        var envJwtSecret = Environment.GetEnvironmentVariable("JWT");
        if (!string.IsNullOrWhiteSpace(envJwtSecret))
        {
            jwtSecret = envJwtSecret;
        }
        var key = Encoding.UTF8.GetBytes(jwtSecret ?? throw new InvalidOperationException("JWT secret not configured"));

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; 
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; 
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = "rokis",
                    ValidAudience = "rokis",
                    ClockSkew = TimeSpan.Zero
                };
                
                // ── дополнительная проверка SecurityStamp ─────────────────
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async ctx =>
                    {
                        var principal = ctx.Principal!;
                        var stampFromToken = principal
                            .FindFirst("AspNet.Identity.SecurityStamp")?.Value;

                        // если claim не найден → отклоняем
                        if (stampFromToken is null)
                        {
                            ctx.Fail("SecurityStamp missing");
                            return;
                        }

                        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                        if (userId is null)
                        {
                            return;
                        }
                        var userMgr = ctx.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                        var user = await userMgr.FindByIdAsync(userId);

                        if (user is null)
                        {
                            ctx.Fail("User not found");
                            return;
                        }

                        var currentStamp = await userMgr.GetSecurityStampAsync(user);

                        if (!string.Equals(stampFromToken, currentStamp, StringComparison.Ordinal))
                        {
                            ctx.Fail("Token security stamp is no longer valid");
                        }
                    }
                };
            });
        builder.Services.AddAuthorization();
        
        builder.Services.ConfigureRepositories();
        builder.Services.ConfigureServices();
        
        // 4. Подключаем контроллеры + Swagger
        builder.Services.AddControllers()
            .ConfigureApiBehaviorOptions(o =>
            {
                o.SuppressModelStateInvalidFilter = true;
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo {
                Title = "Rokis API",
                Description = "Rokis API",
                Version = "v1" });
            
            // Добавим описание схемы Bearer
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            
            // Добавление документации в Swagger
            var xml = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xml));
            
            // Подключаем наш OperationFilter (см. далее)
            c.OperationFilter<AuthorizeCheckOperationFilter>();
        });
    }

    public static void ConfigureRepositories(this IServiceCollection services)
    {
        // Репозиторий конфигурации
        services.AddScoped<IConfigRepository, ConfigRepository>();
        
        // Репозиторий компании
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        // Репозиторий сотрудника
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        // Репозиторий физ лица
        services.AddScoped<IPersonRepository, PersonRepository>();
        
        services.AddScoped<IUserRepository, UserRepository>();
        // Репозиторий авторизации
        services.AddScoped<IRegisterRepository, RegisterRepository>();
        
        // Репозиторий работы с токенами
        services.AddScoped<ITokenRepository, TokenRepository>();
        
        // Репозиторий работы с заказами
        services.AddScoped<IOrderRepository, OrderRepository>();
        
        // Репозиторий сессий
        services.AddScoped<ISessionRepository, SessionRepository>();
        
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<IUserTopicRepository, UserTopicRepository>();
        services.AddScoped<IUserAnswerRepository, UserAnswerRepository>();
        
        services.AddScoped<IReportRepository, ReportRepository>();
    }
    public static void ConfigureServices(this IServiceCollection services)
    {
        // Сервис по генерации Jwt
        services.AddHttpContextAccessor();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        
        // Сервис работы с конфигурациями
        services.AddScoped<IConfigService, ConfigService>();
        
        // Сервисы расчетов
        services.AddScoped<ITimeCalculate, TimeCalculate>();
        services.AddScoped<IScoreCalculate, ScoreCalculate>();
        services.AddScoped<IGradeCalculate, GradeCalculate>();
        
        // Сервис по работе с пользовательскими темами
        services.AddScoped<IUserTopicService, UserTopicService>();
        
        // Сервис по работе с сессией
        services.AddScoped<ISessionService, SessionService>();
        
        // Сервис по работе с вопросами
        services.AddScoped<IQuestionService, QuestionService>();
        
        // Сервис по работе с пользовательскими ответами
        services.AddScoped<IUserAnswerService, UserAnswerService>();
        
        // Расчет параметров темы и вопроса
        services.AddScoped<IScoreService, ScoreService>();
        
        // Сервис отрисовки отчета
        services.AddScoped<IChartService, ChartService>();
        
        // Сервис для расчета метрик
        services.AddScoped<IMetricService, MetricService>();
        
        // Расчет отчета
        services.AddScoped<IReportService, ReportService>();
        
        // Фоновый сервис проверки открытых сессий.
        services.AddSingleton<IDurationProvider, SessionDurationProvider>();
        services.AddHostedService<SessionTimeoutWorker>();
    }
    public static void RegisterMiddlewares(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Rokis API");
        });

        app.UseForwardedHeaders(new ForwardedHeadersOptions {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });
        
        app.UseRouting();
        
        // Метрики Prometheus
        app.UseHttpMetrics();
        
        // Включаем аутентификацию/авторизацию
        app.UseAuthentication();
        app.UseMiddleware<PasswordExpirationMiddleware>();
        app.UseAuthorization();
        
        // Пользовательские middleware
        // app.UseMiddleware<IpUserAgentValidationMiddleware>();
        
        app.MapControllers();
        
        app.MapMetrics();
    }
}