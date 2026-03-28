using AppComercial.Api.Gateway.Middleware;
using AppComercial.Api.Infrastructure;
using AppComercial.Api.Infrastructure.Repositories;
using AppComercial.Api.Sdk;
using AspNetCoreRateLimit;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "CONTPAQi Comercial API Local (Open Source)",
        Version = "v1",
        Description = "API Web hibrida usando EF Core (Lectura rápida) y P/Invoke SDK (Escritura)."
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingrese 'Bearer' [espacio] y luego su token válido."
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Registrar DbContext de Entity Framework Core para Consultas Rápidas SQL.
// Nota: Aquí pasamos el connection string de appsettings.json
var msSqlConnectionString = builder.Configuration.GetConnectionString("ContpaqiComercial");
builder.Services.AddDbContext<ContpaqiDbContext>(options =>
{
    if (string.IsNullOrEmpty(msSqlConnectionString))
    {
        // Fallback a SQL Express base predeterminada
        options.UseSqlServer("Server=localhost\\Compac;Database=adCONTPAQi_Comercial;Trusted_Connection=True;Encrypt=False;",
            options => options.UseCompatibilityLevel(120));
    }
    else 
    {
        options.UseSqlServer(msSqlConnectionString,
            options => options.UseCompatibilityLevel(120));
    }
});

// Registrar DbContext secundario para CompacWAdmin (usuarios activos del sistema CONTPAQi)
var compacWAdminConnectionString = builder.Configuration.GetConnectionString("CompacWAdmin");
builder.Services.AddDbContext<CompacWAdminDbContext>(options =>
{
    var connStr = string.IsNullOrEmpty(compacWAdminConnectionString)
        ? "Server=localhost\\PCOMERCIAL;Database=CompacWAdmin;Trusted_Connection=True;Encrypt=False;"
        : compacWAdminConnectionString;
    options.UseSqlServer(connStr, options => options.UseCompatibilityLevel(120));
});

// Registrar DbContext para RepositorioAdminPAQ (usuarios y perfiles de CONTPAQi)
var repositorioAdminConnectionString = builder.Configuration.GetConnectionString("RepositorioAdminPAQ");
builder.Services.AddDbContext<RepositorioAdminDbContext>(options =>
{
    var connStr = string.IsNullOrEmpty(repositorioAdminConnectionString)
        ? "Server=localhost\\PCOMERCIAL;Database=RepositorioAdminPAQ;Trusted_Connection=True;Encrypt=False;"
        : repositorioAdminConnectionString;
    options.UseSqlServer(connStr, options => options.UseCompatibilityLevel(120));
});

// Registrar MediatR para CQRS
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

// Registrar AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// Registrar FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Registrar Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

// Registrar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Registrar API Versioning
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

// Registrar Repositories
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();

// Registrar Wrapper SDK Open Source (P/Invoke) para Escrituras
builder.Services.AddSingleton<IContpaqiSdk, ContpaqiSdk>();

// Servicio Background para Inicializar la conexión COM con CONTPAQi Comercial
// Descoméntalo si realmente necesitas el SDK activo (para que no choque si no está el Server de CONTPAQi configurado en dev local)
builder.Services.AddHostedService<ContpaqiHostedService>();

// Configurar Autenticación con JWT
var secretKey = builder.Configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured. Set it in appsettings.json or user secrets.");
var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // Solo true en producción/HTTPS
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

// Configure the HTTP request pipeline.
app.UseGlobalExceptionHandler();
app.UseRequestResponseLogging();
app.UseIpRateLimiting();
app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CONTPAQi Comercial API Open Source V1");
        c.RoutePrefix = string.Empty; // Para que Swagger este en la raiz
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
