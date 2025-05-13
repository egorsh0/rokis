using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using idcc.Application;
using idcc.Application.Interfaces;
using idcc.Context;
using idcc.Filters;
using idcc.Infrastructures;
using idcc.Infrastructures.Interfaces;
using idcc.Middlewares;
using idcc.Models;
using idcc.Repository;
using idcc.Repository.Interfaces;
using idcc.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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
        var connectionString = builder.Configuration.GetConnectionString("idccDb");

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
        
        var smtpSettings = builder.Configuration.GetSection("SmtpSettings").Get<SmtpSettings>();
        if (smtpSettings != null)
        {
            builder.Services.AddSingleton(smtpSettings);
            builder.Services.AddScoped<IEmailService, MailKitEmailService>();
            builder.Services.AddScoped<IInviteService, InviteService>();
        }

        // 1. Подключаем EF Core (PostgreSQL)
        builder.Services.AddDbContext<IdccContext>(options =>
        {
            options.UseLazyLoadingProxies();
            options.UseNpgsql(connectionString);
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
        var jwtSecret = builder.Configuration["Jwt:Secret"];
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
                    ValidIssuer = "Idcc",
                    ValidAudience = "Idcc",
                    // При желании можно включить ValidateLifetime = true
                    // и настроить ClockSkew = TimeSpan.Zero, чтобы не было доп. времени
                };
            });
        builder.Services.AddAuthorization();
        
        // Репозиторий компании
        builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
        
        // Репозиторий авторизации
        builder.Services.AddScoped<IAuthRepository, AuthRepository>();
        
        // Репозиторий конфигурации
        builder.Services.AddScoped<IConfigRepository, ConfigRepository>();
        
        // Репозиторий работы с токенами
        builder.Services.AddScoped<ITokenRepository, TokenRepository>();
        
        // Репозиторий сессий
        builder.Services.AddScoped<ISessionRepository, SessionRepository>();
        
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
        builder.Services.AddScoped<IUserTopicRepository, UserTopicRepository>();
        builder.Services.AddScoped<IUserAnswerRepository, UserAnswerRepository>();
        builder.Services.AddScoped<IDataRepository, DataRepository>();

        builder.Services.AddScoped<ITimeCalculate, TimeCalculate>();
        builder.Services.AddScoped<IWeightCalculate, WeightCalculate>();
        builder.Services.AddScoped<IScoreCalculate, ScoreCalculate>();
        builder.Services.AddScoped<IGradeCalculate, GradeCalculate>();
        
        builder.Services.AddScoped<IGraphGenerate, GraphGenerate>();

        builder.Services.AddScoped<IReportRepository, ReportRepository>();
        builder.Services.AddScoped<IIdccApplication, IdccApplication>();
        builder.Services.AddScoped<IIdccReport, IdccReport>();

        // 4. Подключаем контроллеры + Swagger
        builder.Services.AddControllers();
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

    public static void RegisterMiddlewares(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "MVP Аттестация API");
        });

        app.UseHttpsRedirection();
        
        // Включаем аутентификацию/авторизацию
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.UseMiddleware<IpUserAgentValidationMiddleware>();
        
        app.MapControllers();
    }
}