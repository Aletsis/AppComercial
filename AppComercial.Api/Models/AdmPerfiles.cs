using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppComercial.Api.Models;

/// <summary>
/// Representa un Perfil de usuario en CONTPAQi Comercial (tabla admPerfiles).
/// Un perfil define los permisos y accesos que tienen los usuarios asociados.
/// </summary>
[Table("admPerfiles")]
public class AdmPerfiles
{
    [Key]
    [Column("CIDPERFIL")]
    public int Id { get; set; }

    [Column("CCODIGOPERFIL")]
    public string? CodigoPerfil { get; set; }

    [Column("CNOMBREPERFIL")]
    public string? NombrePerfil { get; set; }

    [Column("CDESCRIPCIONPERFIL")]
    public string? Descripcion { get; set; }

    [Column("CESTATUS")]
    public int Estatus { get; set; } // 0 = Activo, 1 = Inactivo
}
