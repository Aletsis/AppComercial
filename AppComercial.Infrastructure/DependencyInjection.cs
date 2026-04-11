using AppComercial.Application.Common.Interfaces;
using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Interfaces.SdkModels;
using AppComercial.Infrastructure.Repositories;
using AppComercial.Infrastructure.Sdk;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppComercial.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var msSqlConnectionString = configuration.GetConnectionString("ContpaqiComercial");
        services.AddDbContext<ContpaqiDbContext>(options =>
        {
            if (string.IsNullOrEmpty(msSqlConnectionString))
            {
                options.UseSqlServer("Server=localhost\\Compac;Database=adCONTPAQi_Comercial;Trusted_Connection=True;Encrypt=False;",
                    options => options.UseCompatibilityLevel(120));
            }
            else
            {
                options.UseSqlServer(msSqlConnectionString,
                    options => options.UseCompatibilityLevel(120));
            }
        });
        services.AddScoped<IContpaqiDbContext>(provider => provider.GetRequiredService<ContpaqiDbContext>());

        var compacWAdminConnectionString = configuration.GetConnectionString("CompacWAdmin");
        services.AddDbContext<CompacWAdminDbContext>(options =>
        {
            var connStr = string.IsNullOrEmpty(compacWAdminConnectionString)
                ? "Server=localhost\\PCOMERCIAL;Database=CompacWAdmin;Trusted_Connection=True;Encrypt=False;"
                : compacWAdminConnectionString;
            options.UseSqlServer(connStr, options => options.UseCompatibilityLevel(120));
        });
        services.AddScoped<ICompacWAdminDbContext>(provider => provider.GetRequiredService<CompacWAdminDbContext>());

        var repositorioAdminConnectionString = configuration.GetConnectionString("RepositorioAdminPAQ");
        services.AddDbContext<RepositorioAdminDbContext>(options =>
        {
            var connStr = string.IsNullOrEmpty(repositorioAdminConnectionString)
                ? "Server=localhost\\PCOMERCIAL;Database=RepositorioAdminPAQ;Trusted_Connection=True;Encrypt=False;"
                : repositorioAdminConnectionString;
            options.UseSqlServer(connStr, options => options.UseCompatibilityLevel(120));
        });
        services.AddScoped<IRepositorioAdminDbContext>(provider => provider.GetRequiredService<RepositorioAdminDbContext>());

        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();

        services.AddSingleton<IContpaqiSdk, ContpaqiSdk>();
        services.AddHostedService<ContpaqiHostedService>();

        return services;
    }
}
