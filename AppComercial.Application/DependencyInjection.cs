using AppComercial.Application.Features.Productos;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AppComercial.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Registrar AutoMapper
        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(GetProductosQuery).Assembly));

        // Registrar MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetProductosQuery).Assembly));

        // Registrar FluentValidation
        services.AddValidatorsFromAssembly(typeof(GetProductosQuery).Assembly);

        return services;
    }
}
