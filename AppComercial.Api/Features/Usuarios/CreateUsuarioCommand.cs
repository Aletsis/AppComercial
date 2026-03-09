using MediatR;
using AppComercial.Api.Models;
using AppComercial.Api.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace AppComercial.Api.Features.Usuarios;

/// <summary>
/// Comando para crear un nuevo Usuario en CONTPAQi Comercial.
/// La escritura se realiza directamente sobre la tabla admUsuarios.
/// </summary>
public class CreateUsuarioCommand : IRequest<int>
{
    /// <summary>
    /// Código único del usuario. Requerido. Máximo 30 caracteres.
    /// Ejemplo: "VENDEDOR01", "ADMIN"
    /// </summary>
    [Required(ErrorMessage = "El código del usuario es requerido.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código debe tener entre 1 y 30 caracteres.")]
    public string CodigoUsuario { get; set; } = string.Empty;

    /// <summary>
    /// Nombre completo del usuario. Requerido. Máximo 60 caracteres.
    /// Ejemplo: "Juan Pérez García"
    /// </summary>
    [Required(ErrorMessage = "El nombre del usuario es requerido.")]
    [StringLength(60, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 60 caracteres.")]
    public string NombreUsuario { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña del usuario. Máximo 30 caracteres.
    /// Se almacena tal como la recibe CONTPAQi (sin encriptar por la API).
    /// </summary>
    [StringLength(30, ErrorMessage = "La contraseña no puede exceder 30 caracteres.")]
    public string Contrasena { get; set; } = string.Empty;

    /// <summary>
    /// ID del perfil de permisos asignado al usuario. Requerido.
    /// Consulte GET /api/Perfiles para ver los perfiles disponibles.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "El ID del perfil debe ser mayor a 0.")]
    public int IdPerfil { get; set; }

    /// <summary>
    /// Correo electrónico del usuario. Opcional. Máximo 100 caracteres.
    /// </summary>
    [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres.")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    public string? Email { get; set; }
}

public class CreateUsuarioCommandHandler : IRequestHandler<CreateUsuarioCommand, int>
{
    private readonly ContpaqiDbContext _dbContext;

    public CreateUsuarioCommandHandler(ContpaqiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> Handle(CreateUsuarioCommand request, CancellationToken cancellationToken)
    {
        var usuario = new AdmUsuarios
        {
            CodigoUsuario = request.CodigoUsuario,
            NombreUsuario = request.NombreUsuario,
            Contrasena    = request.Contrasena,
            IdPerfil      = request.IdPerfil,
            Email         = request.Email,
            Estatus       = 0 // Activo por defecto
        };

        _dbContext.Usuarios.Add(usuario);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return usuario.Id;
    }
}
