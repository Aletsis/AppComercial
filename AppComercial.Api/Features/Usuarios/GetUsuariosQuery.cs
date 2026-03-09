using MediatR;
using AppComercial.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Api.Features.Usuarios;

/// <summary>
/// Consulta para obtener los usuarios con sesión activa en CONTPAQi Comercial.
/// Los usuarios de CONTPAQi no se almacenan en la BD de la empresa — se consultan
/// desde CompacWAdmin.dbo.UsuariosActivos, que registra quiénes están conectados.
/// </summary>
public class GetUsuariosQuery : IRequest<IEnumerable<UsuarioActivoDto>>
{
    /// <summary>Filtra por código de usuario. Ejemplo: "SUPERVISOR", "VENDEDOR1".</summary>
    public string? CodigoUsuario { get; set; }

    /// <summary>Filtra por código de empresa (RutaEmpresa).</summary>
    public string? Empresa { get; set; }
}

/// <summary>DTO de usuario activo en el sistema CONTPAQi.</summary>
public record UsuarioActivoDto(
    int IdUsuario,
    string? CodigoUsuario,
    string? Empresa
);

public class GetUsuariosQueryHandler : IRequestHandler<GetUsuariosQuery, IEnumerable<UsuarioActivoDto>>
{
    private readonly CompacWAdminDbContext _dbContext;

    public GetUsuariosQueryHandler(CompacWAdminDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<UsuarioActivoDto>> Handle(GetUsuariosQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.UsuariosActivos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.CodigoUsuario))
            query = query.Where(u => u.CodigoUsuario == request.CodigoUsuario);

        if (!string.IsNullOrWhiteSpace(request.Empresa))
            query = query.Where(u => u.Empresa == request.Empresa);

        return await query
            .OrderBy(u => u.CodigoUsuario)
            .Select(u => new UsuarioActivoDto(u.IdUsuario, u.CodigoUsuario, u.Empresa))
            .ToListAsync(cancellationToken);
    }
}
