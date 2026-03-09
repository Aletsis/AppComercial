using MediatR;
using AppComercial.Api.Models;
using AppComercial.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Api.Features.Perfiles;

/// <summary>
/// Consulta para obtener los Perfiles de permisos de CONTPAQi Comercial desde RepositorioAdminPAQ.
/// Use el IdPerfil de cada perfil para asignarlo a un usuario.
/// </summary>
public class GetPerfilesQuery : IRequest<IEnumerable<CacPerfil>>
{
    /// <summary>Filtra por descripción del perfil (búsqueda parcial).</summary>
    public string? Descripcion { get; set; }
}

public class GetPerfilesQueryHandler : IRequestHandler<GetPerfilesQuery, IEnumerable<CacPerfil>>
{
    private readonly RepositorioAdminDbContext _dbContext;

    public GetPerfilesQueryHandler(RepositorioAdminDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<CacPerfil>> Handle(GetPerfilesQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Perfiles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Descripcion))
            query = query.Where(p => p.Descripcion != null && p.Descripcion.Contains(request.Descripcion));

        return await query
            .OrderBy(p => p.Descripcion)
            .ToListAsync(cancellationToken);
    }
}
