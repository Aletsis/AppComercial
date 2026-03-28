using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppComercial.Api.Models;

[Table("admClasificaciones")]
public class AdmClasificaciones
{
    [Key]
    [Column("CIDCLASIFICACION")]
    public int Id { get; set; }

    [Column("CNOMBRECLASIFICACION")]
    public string Nombre { get; set; } = string.Empty;
}
