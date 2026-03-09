using AppComercial.Api.Infrastructure;
using AppComercial.Api.Sdk;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
});

// Registrar DbContext de Entity Framework Core para Consultas Rápidas SQL.
// Nota: Aquí pasamos el connection string de appsettings.json
var msSqlConnectionString = builder.Configuration.GetConnectionString("ContpaqiComercial");
builder.Services.AddDbContext<ContpaqiDbContext>(options =>
{
    if (string.IsNullOrEmpty(msSqlConnectionString))
    {
        // Fallback a SQL Express base predeterminada
        options.UseSqlServer("Server=localhost\\Compac;Database=adCONTPAQi_Comercial;Trusted_Connection=True;Encrypt=False;");
    }
    else 
    {
        options.UseSqlServer(msSqlConnectionString);
    }
});

// Registrar DbContext secundario para CompacWAdmin (usuarios activos del sistema CONTPAQi)
var compacWAdminConnectionString = builder.Configuration.GetConnectionString("CompacWAdmin");
builder.Services.AddDbContext<CompacWAdminDbContext>(options =>
{
    var connStr = string.IsNullOrEmpty(compacWAdminConnectionString)
        ? "Server=localhost\\PCOMERCIAL;Database=CompacWAdmin;Trusted_Connection=True;Encrypt=False;"
        : compacWAdminConnectionString;
    options.UseSqlServer(connStr);
});

// Registrar DbContext para RepositorioAdminPAQ (usuarios y perfiles de CONTPAQi)
var repositorioAdminConnectionString = builder.Configuration.GetConnectionString("RepositorioAdminPAQ");
builder.Services.AddDbContext<RepositorioAdminDbContext>(options =>
{
    var connStr = string.IsNullOrEmpty(repositorioAdminConnectionString)
        ? "Server=localhost\\PCOMERCIAL;Database=RepositorioAdminPAQ;Trusted_Connection=True;Encrypt=False;"
        : repositorioAdminConnectionString;
    options.UseSqlServer(connStr);
});

// Registrar MediatR para CQRS
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

// Registrar Wrapper SDK Open Source (P/Invoke) para Escrituras
builder.Services.AddSingleton<IContpaqiSdk, ContpaqiSdk>();

// Servicio Background para Inicializar la conexión COM con CONTPAQi Comercial
// Descoméntalo si realmente necesitas el SDK activo (para que no choque si no está el Server de CONTPAQi configurado en dev local)
builder.Services.AddHostedService<ContpaqiHostedService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CONTPAQi Comercial API Open Source V1");
        c.RoutePrefix = string.Empty; // Para que Swagger este en la raiz
    });
}

app.UseAuthorization();

app.MapControllers();

app.Run();
