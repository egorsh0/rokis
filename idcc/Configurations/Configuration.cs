using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using idcc.Application.Interfaces;
using idcc.Context;
using idcc.Filters;
using idcc.Infrastructures;
using idcc.Infrastructures.Interfaces;
using idcc.Middlewares;
using idcc.Models;
using idcc.Providers;
using idcc.Repository;
using idcc.Repository.Interfaces;
using idcc.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Prometheus;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace idcc.Configurations;

public static class Configuration
{
    public static void RegisterServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions();
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        
        string ReadSecret(string value)
        {
            if (File.Exists(value))
            {
                return File.ReadAllText(value).Trim();
            }
            return value;
        }
        
        var rawConnectionString = builder.Configuration.GetConnectionString("idccDb") ?? throw new InvalidOperationException("Connection string not configured");
        var resolvedConnectionString = rawConnectionString.Replace(
            "Password=/run/secrets/postgres_password",
            $"Password={ReadSecret("/run/secrets/postgres_password")}"
        );
        var jwtSecret = ReadSecret(builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret not configured"));
        var smtpSettings = builder.Configuration.GetSection("Smtp").Get<SmtpSettings>();

        // 0. Подключаем кэширование.
        builder.Services.AddFusionCache()
            .WithDefaultEntryOptions(options => options.Duration = TimeSpan.FromMinutes(5))
            .WithSerializer(new FusionCacheSystemTextJsonSerializer())
            .WithDistributedCache(
                new RedisCache(new RedisCacheOptions
                {
                    Configuration = builder.Configuration.GetConnectionString("RadisConnection")
                })
            ).AsHybridCache();
        
        // 0.0 Подключаем рассылки
        if (smtpSettings != null)
        {
            smtpSettings.Password = ReadSecret(smtpSettings.Password ?? throw new InvalidOperationException("SMTP password not configured"));
            builder.Services.AddSingleton(smtpSettings);
            builder.Services.AddScoped<IEmailService, MailKitEmailService>();
            builder.Services.AddScoped<IInviteService, InviteService>();
        }

        // 1. Подключаем EF Core (PostgreSQL)
        builder.Services.AddDbContext<IdccContext>(options =>
        {
            options.UseLazyLoadingProxies();
            options.UseNpgsql(resolvedConnectionString, npg =>
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
            .AddEntityFrameworkStores<IdccContext>()
            .AddDefaultTokenProviders();

        // 3. Подключаем аутентификацию через JWT

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; 
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; 
            })
            .AddJwtBearer(options =>
            {
                var key = Encoding.UTF8.GetBytes(jwtSecret);
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = "Idcc",
                    ValidAudience = "Idcc",
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
                Title = "MVP Аттестация API",
                Description = "MVP API",
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
        // Репозиторий компании
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        // Репозиторий сотрудника
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        // Репозиторий физ лица
        services.AddScoped<IPersonRepository,   PersonRepository>();
        
        // Репозиторий авторизации
        services.AddScoped<IRegisterRepository, RegisterRepository>();
        
        // Репозиторий конфигурации
        services.AddScoped<IConfigRepository, ConfigRepository>();
        
        // Репозиторий работы с токенами
        services.AddScoped<ITokenRepository, TokenRepository>();
        
        // Репозиторий работы с заказами
        services.AddScoped<IOrderRepository, OrderRepository>();
        
        // Репозиторий сессий
        services.AddScoped<ISessionRepository, SessionRepository>();
        
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<IUserTopicRepository, UserTopicRepository>();
        services.AddScoped<IUserAnswerRepository, UserAnswerRepository>();
        services.AddScoped<IDataRepository, DataRepository>();
        
        services.AddScoped<IReportRepository, ReportRepository>();
    }
    public static void ConfigureServices(this IServiceCollection services)
    {
        // Сервис по генерации Jwt
        services.AddHttpContextAccessor();
        services.AddScoped<ITokenService, TokenService>();
        
        // Сервис по работе с сессией
        services.AddScoped<ISessionService, SessionService>();
        
        // Сервисы расчетов
        services.AddScoped<ITimeCalculate, TimeCalculate>();
        services.AddScoped<IScoreCalculate, ScoreCalculate>();
        services.AddScoped<IGradeCalculate, GradeCalculate>();
        
        // Расчет параметров темы и вопроса
        services.AddScoped<IScoreService, ScoreService>();
        
        // Сервис отрисовки отчета
        services.AddScoped<IGraphService, GraphService>();
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
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "MVP Аттестация API");
        });

        app.UseHttpsRedirection();
        
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