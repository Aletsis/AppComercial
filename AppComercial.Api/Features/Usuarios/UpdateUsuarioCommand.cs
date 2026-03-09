using MediatR;
using AppComercial.Api.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace AppComercial.Api.Features.Usuarios;

/// <summary>
/// Comando para actualizar datos de un Usuario existente en CONTPAQi Comercial.
/// El ID del usuario se proporciona en la URL. Al menos un campo debe ser enviado.
/// </summary>
public class UpdateUsuarioCommand : IRequest<bool>
{
    /// <summary>ID interno del usuario. Se asigna desde la URL.</summary>
    public int Id { get; set; }

    /// <summary>Nuevo nombre completo. Opcional. Máximo 60 caracteres.</summary>
    [StringLength(60, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 60 caracteres.")]
    public string? NombreUsuario { get; set; }

    /// <summary>
    /// Nueva contraseña. Opcional. Máximo 30 caracteres.
    /// Se almacena tal como la recibe CONTPAQi.
    /// </summary>
    [StringLength(30, ErrorMessage = "La contraseña no puede exceder 30 caracteres.")]
    public string? Contrasena { get; set; }

    /// <summary>
    /// Nuevo ID de perfil de permisos. Opcional.
    /// Consulte GET /api/Perfiles para ver los disponibles.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "El ID del perfil debe ser mayor a 0.")]
    public int? IdPerfil { get; set; }

    /// <summary>Nuevo email del usuario. Opcional. Máximo 100 caracteres.</summary>
    [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres.")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    public string? Email { get; set; }

    /// <summary>
    /// Nuevo estado del usuario. Opcional.
    /// Valores: 0 = Activo, 1 = Inactivo.
    /// </summary>
    [Range(0, 1, ErrorMessage = "Estatus debe ser 0 (Activo) o 1 (Inactivo).")]
    public int? Estatus { get; set; }
}

public class UpdateUsuarioCommandHandler : IRequestHandler<UpdateUsuarioCommand, bool>
{
    private readonly ContpaqiDbContext _dbContext;

    public UpdateUsuarioCommandHandler(ContpaqiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> Handle(UpdateUsuarioCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _dbContext.Usuarios.FindAsync(new object[] { request.Id }, cancellationToken);

        if (usuario is null)
            throw new KeyNotFoundException($"No se encontró el usuario con ID {request.Id}.");

        if (request.NombreUsuario is not null)
            usuario.NombreUsuario = request.NombreUsuario;

        if (request.Contrasena is not null)
            usuario.Contrasena = request.Contrasena;

        if (request.IdPerfil.HasValue)
            usuario.IdPerfil = request.IdPerfil.Value;

        if (request.Email is not null)
            usuario.Email = request.Email;

        if (request.Estatus.HasValue)
            usuario.Estatus = request.Estatus.Value;

        var cambios = await _dbContext.SaveChangesAsync(cancellationToken);
        return cambios > 0;
    }
}
