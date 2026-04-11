using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppComercial.Domain.Entities;

/// <summary>
/// Representa un usuario con sesión activa en CONTPAQi Comercial.
/// Tabla: CompacWAdmin.dbo.UsuariosActivos
/// Esta tabla se actualiza en tiempo real cuando los usuarios abren/cierran sesión.
/// </summary>
[Table("UsuariosActivos")]
public class AdmUsuariosActivos
{
    [Key]
    [Column("CIDUSUARIO")]
    public int IdUsuario { get; set; }

    [Column("CUSUARIO")]
    public string? CodigoUsuario { get; set; }

    [Column("CEMPRESA")]
    public string? Empresa { get; set; }
}
