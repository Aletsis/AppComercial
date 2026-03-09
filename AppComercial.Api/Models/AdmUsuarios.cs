using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppComercial.Api.Models;

/// <summary>
/// Representa un usuario del sistema en CONTPAQi Comercial (tabla admUsuarios).
/// </summary>
[Table("admUsuarios")]
public class AdmUsuarios
{
    [Key]
    [Column("CIDUSUARIO")]
    public int Id { get; set; }

    [Column("CCODIGOUSUARIO")]
    public string? CodigoUsuario { get; set; }

    [Column("CNOMBREUSUARIO")]
    public string? NombreUsuario { get; set; }

    [Column("CIDPERFIL")]
    public int IdPerfil { get; set; }

    [Column("CCONTRASENA")]
    public string? Contrasena { get; set; }

    [Column("CESTATUS")]
    public int Estatus { get; set; } // 0 = Activo, 1 = Inactivo

    [Column("CEMAIL")]
    public string? Email { get; set; }

    [Column("CTEXTOEXTRA1")]
    public string? TextoExtra1 { get; set; }

    [Column("CTEXTOEXTRA2")]
    public string? TextoExtra2 { get; set; }

    [Column("CTEXTOEXTRA3")]
    public string? TextoExtra3 { get; set; }

    [Column("CFECHAEXTRA")]
    public DateTime? FechaExtra { get; set; }
}
