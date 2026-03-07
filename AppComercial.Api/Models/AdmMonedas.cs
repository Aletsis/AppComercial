using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppComercial.Api.Models;

[Table("admMonedas")]
public class AdmMonedas
{
    [Key]
    [Column("CIDMONEDA")]
    public int Id { get; set; }

    [Column("CNOMBREMONEDA")]
    public string NombreMoneda { get; set; } = string.Empty;

    [Column("CSIMBOLOMONEDA")]
    public string Simbolo { get; set; } = string.Empty;

    [Column("CPLURAL")]
    public string Plural { get; set; } = string.Empty;

    [Column("CSINGULAR")]
    public string Singular { get; set; } = string.Empty;
}
