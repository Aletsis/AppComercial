using MediatR;
using AppComercial.Api.Models;
using AppComercial.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Api.Features.Usuarios;

/// <summary>
/// Consulta para obtener la lista de Usuarios del sistema CONTPAQi Comercial.
/// Lee directamente de la tabla admUsuarios (solo lectura).
/// Las contraseñas nunca se retornan en la respuesta.
/// </summary>
public class GetUsuariosQuery : IRequest<IEnumerable<UsuarioDto>>
{
    /// <summary>Filtra por código de usuario. Ejemplo: "ADMIN", "VENDEDOR1".</summary>
    public string? CodigoUsuario { get; set; }

    /// <summary>
    /// Filtra por estado. 
    /// Valores: 0 = Activos (default), 1 = Inactivos, null = Todos.
    /// </summary>
    public int? Estatus { get; set; } = 0;

    /// <summary>Filtra por ID del perfil asignado.</summary>
    public int? IdPerfil { get; set; }
}

/// <summary>DTO de Usuario. Excluye la contraseña por seguridad.</summary>
public record UsuarioDto(
    int Id,
    string? CodigoUsuario,
    string? NombreUsuario,
    int IdPerfil,
    int Estatus,
    string? Email
);

public class GetUsuariosQueryHandler : IRequestHandler<GetUsuariosQuery, IEnumerable<UsuarioDto>>
{
    private readonly ContpaqiDbContext _dbContext;

    public GetUsuariosQueryHandler(ContpaqiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<UsuarioDto>> Handle(GetUsuariosQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Usuarios.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.CodigoUsuario))
            query = query.Where(u => u.CodigoUsuario == request.CodigoUsuario);

        if (request.Estatus.HasValue)
            query = query.Where(u => u.Estatus == request.Estatus.Value);

        if (request.IdPerfil.HasValue)
            query = query.Where(u => u.IdPerfil == request.IdPerfil.Value);

        return await query
            .OrderBy(u => u.CodigoUsuario)
            .Select(u => new UsuarioDto(
                u.Id,
                u.CodigoUsuario,
                u.NombreUsuario,
                u.IdPerfil,
                u.Estatus,
                u.Email
            ))
            .ToListAsync(cancellationToken);
    }
}
