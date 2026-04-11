using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppComercial.Domain.Entities;

/// <summary>
/// Representa un Perfil de permisos de CONTPAQi Comercial.
/// Tabla: RepositorioAdminPAQ.dbo.CAC30000
/// Los usuarios se asocian a perfiles mediante CacUsuario.IdPerfil.
/// </summary>
[Table("CAC30000")]
public class CacPerfil
{
    [Key]
    [Column("IDPERFIL")]
    public int IdPerfil { get; set; }

    /// <summary>Nombre o descripción del perfil. Máximo 30 caracteres.</summary>
    [Column("DESCRIPCION")]
    public string? Descripcion { get; set; }
}
