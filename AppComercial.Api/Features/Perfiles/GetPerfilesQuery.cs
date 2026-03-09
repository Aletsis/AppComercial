using MediatR;
using AppComercial.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Api.Features.Perfiles;

/// <summary>
/// Consulta para obtener la lista de empresas registradas en CONTPAQi.
/// Los "perfiles" de usuario en CONTPAQi no se almacenan en tablas SQL accesibles
/// directamente — se gestionan internamente desde la aplicación CONTPAQi.
/// Este endpoint retorna las empresas disponibles en el sistema (tabla Empresas de CompacWAdmin).
/// </summary>
public class GetEmpresasQuery : IRequest<IEnumerable<EmpresaDto>>
{
    /// <summary>Filtra por nombre de empresa.</summary>
    public string? Nombre { get; set; }
}

/// <summary>DTO con información de una empresa registrada en CONTPAQi.</summary>
public record EmpresaDto(
    int Id,
    string? Nombre,
    string? Ruta
);

public class GetEmpresasQueryHandler : IRequestHandler<GetEmpresasQuery, IEnumerable<EmpresaDto>>
{
    private readonly CompacWAdminDbContext _dbContext;

    public GetEmpresasQueryHandler(CompacWAdminDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<EmpresaDto>> Handle(GetEmpresasQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Empresas.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Nombre))
            query = query.Where(e => e.Nombre != null && e.Nombre.Contains(request.Nombre));

        return await query
            .OrderBy(e => e.Nombre)
            .Select(e => new EmpresaDto(e.Id, e.Nombre, e.Ruta))
            .ToListAsync(cancellationToken);
    }
}
