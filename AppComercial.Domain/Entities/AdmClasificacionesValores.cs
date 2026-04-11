using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppComercial.Domain.Entities;

[Table("admClasificacionesValores")]
public class AdmClasificacionesValores
{
    [Key]
    [Column("CIDVALORCLASIFICACION")]
    public int Id { get; set; }

    [Column("CIDCLASIFICACION")]
    public int ClasificacionId { get; set; }

    [Column("CCODIGOVALORCLASIFICACION")]
    public string CodigoValorClasificacion { get; set; } = string.Empty;

    [Column("CVALORCLASIFICACION")]
    public string ValorClasificacion { get; set; } = string.Empty;
}
