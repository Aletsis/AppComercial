using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppComercial.Api.Models;

[Table("admUnidadesMedidaPeso")]
public class AdmUnidadesMedidaPeso
{
    [Key]
    [Column("CIDUNIDAD")]
    public int Id { get; set; }

    [Column("CNOMBREUNIDAD")]
    public string NombreUnidad { get; set; } = string.Empty;

    [Column("CABREVIATURA")]
    public string Abreviatura { get; set; } = string.Empty;

    [Column("CCLAVESAT")]
    public string ClaveSat { get; set; } = string.Empty;
}
