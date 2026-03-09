using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppComercial.Api.Models;

/// <summary>
/// Representa un Usuario del sistema CONTPAQi Comercial.
/// Tabla: RepositorioAdminPAQ.dbo.CAC10000
/// </summary>
[Table("CAC10000")]
public class CacUsuario
{
    /// <summary>ID autoincremental interno de la fila.</summary>
    [Key]
    [Column("CIDAUTOINCSQL")]
    public int IdAutoInc { get; set; }

    /// <summary>ID del usuario (puede repetirse por sistema).</summary>
    [Column("IDUSUARIO")]
    public int IdUsuario { get; set; }

    /// <summary>Identificador del sistema PAQ (ej. "5" = CONTPAQi Comercial).</summary>
    [Column("IDSISTEMA")]
    public string? IdSistema { get; set; }

    /// <summary>Nombre de usuario (login). Máximo 15 caracteres.</summary>
    [Column("CLAVE")]
    public string? Clave { get; set; }

    /// <summary>Nombre descriptivo del usuario. Máximo 30 caracteres.</summary>
    [Column("NOMBRE")]
    public string? Nombre { get; set; }

    /// <summary>Hash de la contraseña. No se expone en las respuestas de la API.</summary>
    [Column("PASSWORD")]
    public string? Password { get; set; }

    /// <summary>Fecha de alta del usuario.</summary>
    [Column("FECHAALTA")]
    public DateTime? FechaAlta { get; set; }

    /// <summary>Fecha de vencimiento de la cuenta (null = sin vencimiento).</summary>
    [Column("FECHAVENCIMIENTO")]
    public DateTime? FechaVencimiento { get; set; }

    /// <summary>GUID único del usuario en el repositorio.</summary>
    [Column("GUIDUSUARIO")]
    public string? GuidUsuario { get; set; }

    /// <summary>ID del perfil de permisos asignado al usuario.</summary>
    [Column("NIVEL")]
    public int? IdPerfil { get; set; }
}
