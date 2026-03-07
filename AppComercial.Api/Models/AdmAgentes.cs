using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppComercial.Api.Models;

[Table("admAgentes")]
public class AdmAgentes
{
    [Key]
    [Column("CIDAGENTE")]
    public int Id { get; set; }

    [Column("CCODIGOAGENTE")]
    public string CodigoAgente { get; set; } = string.Empty;

    [Column("CNOMBREAGENTE")]
    public string NombreAgente { get; set; } = string.Empty;

    [Column("CTIPOAGENTE")]
    public int TipoAgente { get; set; }
}
