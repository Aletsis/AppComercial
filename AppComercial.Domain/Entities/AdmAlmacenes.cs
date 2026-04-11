using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppComercial.Domain.Entities;

[Table("admAlmacenes")]
public class AdmAlmacenes
{
    [Key]
    public int CIDALMACEN { get; set; }

    [Column("CCODIGOALMACEN")]
    public string? CCODIGOALMACEN { get; set; }

    [Column("CNOMBREALMACEN")]
    public string? CNOMBREALMACEN { get; set; }

    [Column("CFECHAALTAALMACEN")]
    public DateTime? CFECHAALTAALMACEN { get; set; }

    [Column("CIDVALORCLASIFICACION1")]
    public int CIDVALORCLASIFICACION1 { get; set; }

    [Column("CIDVALORCLASIFICACION2")]
    public int CIDVALORCLASIFICACION2 { get; set; }

    [Column("CIDVALORCLASIFICACION3")]
    public int CIDVALORCLASIFICACION3 { get; set; }

    [Column("CIDVALORCLASIFICACION4")]
    public int CIDVALORCLASIFICACION4 { get; set; }

    [Column("CIDVALORCLASIFICACION5")]
    public int CIDVALORCLASIFICACION5 { get; set; }

    [Column("CIDVALORCLASIFICACION6")]
    public int CIDVALORCLASIFICACION6 { get; set; }

    [Column("CSEGCONTALMACEN")]
    public string? CSEGCONTALMACEN { get; set; }

    [Column("CTEXTOEXTRA1")]
    public string? CTEXTOEXTRA1 { get; set; }

    [Column("CTEXTOEXTRA2")]
    public string? CTEXTOEXTRA2 { get; set; }

    [Column("CTEXTOEXTRA3")]
    public string? CTEXTOEXTRA3 { get; set; }

    [Column("CFECHAEXTRA")]
    public DateTime? CFECHAEXTRA { get; set; }

    [Column("CIMPORTEEXTRA1")]
    public double CIMPORTEEXTRA1 { get; set; }

    [Column("CIMPORTEEXTRA2")]
    public double CIMPORTEEXTRA2 { get; set; }

    [Column("CIMPORTEEXTRA3")]
    public double CIMPORTEEXTRA3 { get; set; }

    [Column("CIMPORTEEXTRA4")]
    public double CIMPORTEEXTRA4 { get; set; }

    [Column("CBANDOMICILIO")]
    public int CBANDOMICILIO { get; set; }

    [Column("CTIMESTAMP")]
    public string? CTIMESTAMP { get; set; }

    [Column("CSCALMAC2")]
    public string? CSCALMAC2 { get; set; }

    [Column("CSCALMAC3")]
    public string? CSCALMAC3 { get; set; }

    [Column("CSISTORIG")]
    public int CSISTORIG { get; set; }
}
