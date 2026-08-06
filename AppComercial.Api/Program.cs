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
using AppComercial.Api.Gateway.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

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

        // Directorio de configuración en ProgramData
        var programDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AppComercial");
        var apiConfigPath = Path.Combine(programDataDir, "Api", "appsettings.json");

        // Si existe el appsettings.json en ProgramData, agregarlo como proveedor principal de configuración
        if (File.Exists(apiConfigPath))
        {
            builder.Configuration.AddJsonFile(apiConfigPath, optional: true, reloadOnChange: true);
        }
        
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
        builder.Services.AddControllers(options =>
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            options.Filters.Add(new AuthorizeFilter(policy));
        })
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
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUserService, AppComercial.Api.Services.CurrentUserService>();

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

        // ── JWT Secret: autogenerar si está vacío (igual que ApiKey) ────────────────
        // IMPORTANTE: debe hacerse ANTES de builder.Build() para que el mismo
        // secreto se use en la configuración de autenticación Y quede guardado en disco.
        var secretKey = builder.Configuration["JwtSettings:SecretKey"];
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            secretKey = $"{Guid.NewGuid()}_{Guid.NewGuid()}_SuperSecretKeyForProductionLongEnough256Bits";
            var jwtSettingsPath = File.Exists(apiConfigPath) ? apiConfigPath : Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            try
            {
                string jwtJson = File.Exists(jwtSettingsPath) ? File.ReadAllText(jwtSettingsPath) : "{}";
                var jwtRoot = System.Text.Json.Nodes.JsonNode.Parse(jwtJson)?.AsObject();
                if (jwtRoot != null)
                {
                    if (jwtRoot["JwtSettings"] == null)
                        jwtRoot["JwtSettings"] = new System.Text.Json.Nodes.JsonObject();
                    jwtRoot["JwtSettings"]!["SecretKey"] = secretKey;
                    File.WriteAllText(jwtSettingsPath,
                        jwtRoot.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                    logAction?.Invoke($"[SEGURIDAD] JWT Secret autogenerado y guardado en appsettings.json.");
                }
            }
            catch (Exception ex)
            {
                logAction?.Invoke($"[SEGURIDAD WARN] No se pudo persistir el JWT Secret: {ex.Message}");
            }
        }
        var key = Encoding.UTF8.GetBytes(secretKey);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = "ApiKeyOrJwt";
            options.DefaultChallengeScheme = "ApiKeyOrJwt";
        })
        .AddPolicyScheme("ApiKeyOrJwt", "ApiKeyOrJwt", options =>
        {
            options.ForwardDefaultSelector = context =>
            {
                if (context.Request.Headers.ContainsKey("X-Api-Key"))
                {
                    return "ApiKey";
                }
                return JwtBearerDefaults.AuthenticationScheme;
            };
        })
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
        })
        .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>("ApiKey", null);

        var app = builder.Build();

        // ── Autogenerar API Key de seguridad en el primer arranque ────────────────────────
        var config = app.Services.GetRequiredService<IConfiguration>();
        var apiKey = config["ApiSettings:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            var newKey = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
            var appSettingsPath = File.Exists(apiConfigPath) ? apiConfigPath : Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            try
            {
                string jsonContent = File.Exists(appSettingsPath) ? File.ReadAllText(appSettingsPath) : "{}";
                var rootNode = System.Text.Json.Nodes.JsonNode.Parse(jsonContent)?.AsObject();
                if (rootNode != null)
                {
                    var apiSettings = rootNode["ApiSettings"]?.AsObject();
                    if (apiSettings == null)
                    {
                        apiSettings = new System.Text.Json.Nodes.JsonObject();
                        rootNode["ApiSettings"] = apiSettings;
                    }

                    apiSettings["ApiKey"] = newKey;
                    if (apiSettings["RequireApiKeyForLogin"] == null)
                    {
                        apiSettings["RequireApiKeyForLogin"] = false;
                    }

                    File.WriteAllText(appSettingsPath, rootNode.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                    logAction?.Invoke($"[SEGURIDAD] API Key autogenerada de forma segura y guardada en appsettings.json: {newKey}");
                }
            }
            catch (Exception ex)
            {
                logAction?.Invoke($"[SEGURIDAD ERROR] Error al guardar la API Key autogenerada: {ex.Message}");
            }
        }


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
