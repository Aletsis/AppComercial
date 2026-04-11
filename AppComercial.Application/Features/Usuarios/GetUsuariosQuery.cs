using MediatR;
using AppComercial.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Application.Features.Usuarios;

/// <summary>
/// Consulta para obtener los Usuarios de CONTPAQi Comercial desde RepositorioAdminPAQ.
/// Filtra por IDSISTEMA='5' que corresponde a CONTPAQi Comercial.
/// Las contraseñas NUNCA se retornan en la respuesta.
/// </summary>
public class GetUsuariosQuery : IRequest<IEnumerable<UsuarioDto>>
{
    /// <summary>Filtra por clave de usuario (login). Ejemplo: "SUPERVISOR", "VENDEDOR1".</summary>
    public string? Clave { get; set; }

    /// <summary>Filtra por nombre del usuario.</summary>
    public string? Nombre { get; set; }

    /// <summary>Filtra por ID de perfil asignado.</summary>
    public int? IdPerfil { get; set; }
}

/// <summary>DTO de Usuario — excluye la contraseña por seguridad.</summary>
public record UsuarioDto(
    int IdUsuario,
    string? Clave,
    string? Nombre,
    int? IdPerfil,
    DateTime? FechaAlta,
    DateTime? FechaVencimiento
);

public class GetUsuariosQueryHandler : IRequestHandler<GetUsuariosQuery, IEnumerable<UsuarioDto>>
{
    private readonly IRepositorioAdminDbContext _dbContext;

    public GetUsuariosQueryHandler(IRepositorioAdminDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<UsuarioDto>> Handle(GetUsuariosQuery request, CancellationToken cancellationToken)
    {
        // IDSISTEMA = "5" corresponde a CONTPAQi Comercial
        var query = _dbContext.Usuarios
            .AsNoTracking()
            .Where(u => u.IdSistema == "5");

        if (!string.IsNullOrWhiteSpace(request.Clave))
            query = query.Where(u => u.Clave == request.Clave);

        if (!string.IsNullOrWhiteSpace(request.Nombre))
            query = query.Where(u => u.Nombre != null && u.Nombre.Contains(request.Nombre));

        if (request.IdPerfil.HasValue)
            query = query.Where(u => u.IdPerfil == request.IdPerfil.Value);

        return await query
            .OrderBy(u => u.Clave)
            .Select(u => new UsuarioDto(
                u.IdUsuario,
                u.Clave,
                u.Nombre,
                u.IdPerfil,
                u.FechaAlta,
                u.FechaVencimiento
            ))
            .ToListAsync(cancellationToken);
    }
}
