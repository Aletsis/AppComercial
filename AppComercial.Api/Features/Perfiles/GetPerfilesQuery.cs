using MediatR;
using AppComercial.Api.Models;
using AppComercial.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Api.Features.Perfiles;

/// <summary>
/// Consulta para obtener los Perfiles de usuario de CONTPAQi Comercial.
/// Los perfiles definen los permisos y accesos de los usuarios.
/// </summary>
public class GetPerfilesQuery : IRequest<IEnumerable<AdmPerfiles>>
{
    /// <summary>Filtra por código de perfil. Ejemplo: "ADMIN", "VENDEDOR".</summary>
    public string? CodigoPerfil { get; set; }

    /// <summary>
    /// Filtra por estado. 0 = Activos (default), 1 = Inactivos, null = Todos.
    /// </summary>
    public int? Estatus { get; set; } = 0;
}

public class GetPerfilesQueryHandler : IRequestHandler<GetPerfilesQuery, IEnumerable<AdmPerfiles>>
{
    private readonly ContpaqiDbContext _dbContext;

    public GetPerfilesQueryHandler(ContpaqiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<AdmPerfiles>> Handle(GetPerfilesQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Perfiles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.CodigoPerfil))
            query = query.Where(p => p.CodigoPerfil == request.CodigoPerfil);

        if (request.Estatus.HasValue)
            query = query.Where(p => p.Estatus == request.Estatus.Value);

        return await query
            .OrderBy(p => p.CodigoPerfil)
            .ToListAsync(cancellationToken);
    }
}
