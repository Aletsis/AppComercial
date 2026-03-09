using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppComercial.Api.Models;

/// <summary>
/// Representa una empresa registrada en el sistema CONTPAQi Comercial.
/// Tabla: CompacWAdmin.dbo.Empresas
/// </summary>
[Table("Empresas")]
public class AdmEmpresas
{
    [Key]
    [Column("CIDEMPRESA")]
    public int Id { get; set; }

    [Column("CNOMBREEMPRESA")]
    public string? Nombre { get; set; }

    [Column("CRUTADATOS")]
    public string? Ruta { get; set; }

    [Column("CRUTARESPALDOS")]
    public string? RutaRespaldos { get; set; }
}
