using AppComercial.Api.Gateway.Middleware;
using AppComercial.Application;
using AppComercial.Infrastructure;
using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Interfaces.SdkModels;
using AspNetCoreRateLimit;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AppComercial.Application.Features.Productos;
using AppComercial.Infrastructure.Repositories;
using AppComercial.Infrastructure.Sdk;
using AppComercial.Application.Common.Interfaces;
using Microsoft.OpenApi.Models;
using System.Runtime.Versioning;

[assembly: SupportedOSPlatform("windows")]

// Punto de entrada si se ejecuta la API sola
var logDir = @"C:\AppComercialLogs";
if (!Directory.Exists(logDir)) try { Directory.CreateDirectory(logDir); } catch { }
var logPath = Path.Combine(logDir, "ApiServer.log");

var app = ApiServer.CreateServer(args, msg => {
    var fullMsg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}\n";
    try { File.AppendAllText(logPath, fullMsg); } catch { }
    Console.Write(fullMsg);
});
app.Run();

[SupportedOSPlatform("windows")]
public static class ApiServer
{
    public static WebApplication CreateServer(string[] args, Action<string>? logAction = null)
    {
        // Al correr como Servicio de Windows, el directorio de inicio suele ser C:\Windows\System32
        // Forzamos explícitamente el origen de ejecución y de contenido:
        var options = new WebApplicationOptions
        {
            Args = args,
            ContentRootPath = AppContext.BaseDirectory
        };
        var builder = WebApplication.CreateBuilder(options);
        
        // Configurar la URL de escucha desde appsettings.json (permitiendo acceso externo si se usa http://*:5271)
        var listenUrl = builder.Configuration["ApiSettings:ListenUrl"] ?? "http://*:5271";
        builder.WebHost.UseUrls(listenUrl);

        // La API ya no corre como Servicio de Windows para evitar conflictos (Session 0 isolation)
        // con los componentes COM nativos del SDK de CONTPAQi.
        // Ahora es administrada puramente por el ServerManager en la sesión activa del usuario.

        if (logAction != null)
        {
            builder.Logging.AddProvider(new ActionLoggerProvider(logAction));
        }

        // Add services to the container.
        builder.Services.AddControllers()
               .AddApplicationPart(typeof(ApiServer).Assembly); // Forzar carga de controladores

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "CONTPAQi Comercial API Local (Open Source)",
                Version = "v1",
                Description = "API Web hibrida usando EF Core (Lectura rápida) y P/Invoke SDK (Escritura)."
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Ingrese 'Bearer' [espacio] y luego su token válido."
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
        });

        builder.Services.AddApplicationServices();
        builder.Services.AddInfrastructureServices(builder.Configuration);

        builder.Services.AddMemoryCache();
        builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
        builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy => { policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); });
        });

        builder.Services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ReportApiVersions = true;
        });

        builder.Services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        var secretKey = builder.Configuration["JwtSettings:SecretKey"] ?? "SUPER_CLAVE_SECRETA_LARGA_123456789";
        var key = Encoding.UTF8.GetBytes(secretKey);

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        var app = builder.Build();

        app.UseGlobalExceptionHandler();
        app.UseRequestResponseLogging();
        app.UseIpRateLimiting();
        app.UseCors("AllowAll");

        if (app.Environment.IsDevelopment() || true) // Permitir Swagger siempre en app desktop
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CONTPAQi Comercial API Open Source V1");
                c.RoutePrefix = string.Empty;
            });
        }

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
}

public class ActionLoggerProvider : ILoggerProvider
{
    private readonly Action<string> _logAction;
    public ActionLoggerProvider(Action<string> logAction) => _logAction = logAction;
    public ILogger CreateLogger(string categoryName) => new ActionLogger(categoryName, _logAction);
    public void Dispose() { }
}

public class ActionLogger : ILogger
{
    private readonly string _categoryName;
    private readonly Action<string> _logAction;
    public ActionLogger(string categoryName, Action<string> logAction)
    {
        _categoryName = categoryName;
        _logAction = logAction;
    }
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;
        var msg = formatter(state, exception);
        if (exception != null) msg += $"\n{exception}";
        // Limitar la verbosidad
        if (_categoryName.StartsWith("Microsoft.AspNetCore") || _categoryName.StartsWith("Microsoft.EntityFrameworkCore.Database.Command")) 
        {
            if (logLevel >= LogLevel.Warning)
                _logAction($"[{logLevel}] {_categoryName}: {msg}");
        }
        else
        {
            _logAction($"[{logLevel}] {msg}");
        }
    }
}
