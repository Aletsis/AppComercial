using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppComercial.Api.Models;

[Table("admMovimientos")]
public class AdmMovimientos
{
    [Key]
    public int CIDMOVIMIENTO { get; set; }

    [Column("CIDDOCUMENTO")]
    public int CIDDOCUMENTO { get; set; }

    [Column("CNUMEROMOVIMIENTO")]
    public double CNUMEROMOVIMIENTO { get; set; }

    [Column("CIDDOCUMENTODE")]
    public int CIDDOCUMENTODE { get; set; }

    [Column("CIDPRODUCTO")]
    public int CIDPRODUCTO { get; set; }

    [Column("CIDALMACEN")]
    public int CIDALMACEN { get; set; }

    [Column("CUNIDADES")]
    public double CUNIDADES { get; set; }

    [Column("CUNIDADESNC")]
    public double CUNIDADESNC { get; set; }

    [Column("CUNIDADESCAPTURADAS")]
    public double CUNIDADESCAPTURADAS { get; set; }

    [Column("CIDUNIDAD")]
    public int CIDUNIDAD { get; set; }

    [Column("CIDUNIDADNC")]
    public int CIDUNIDADNC { get; set; }

    [Column("CPRECIO")]
    public double CPRECIO { get; set; }

    [Column("CPRECIOCAPTURADO")]
    public double CPRECIOCAPTURADO { get; set; }

    [Column("CCOSTOCAPTURADO")]
    public double CCOSTOCAPTURADO { get; set; }

    [Column("CCOSTOESPECIFICO")]
    public double CCOSTOESPECIFICO { get; set; }

    [Column("CNETO")]
    public double CNETO { get; set; }

    [Column("CIMPUESTO1")]
    public double CIMPUESTO1 { get; set; }

    [Column("CPORCENTAJEIMPUESTO1")]
    public double CPORCENTAJEIMPUESTO1 { get; set; }

    [Column("CIMPUESTO2")]
    public double CIMPUESTO2 { get; set; }

    [Column("CPORCENTAJEIMPUESTO2")]
    public double CPORCENTAJEIMPUESTO2 { get; set; }

    [Column("CIMPUESTO3")]
    public double CIMPUESTO3 { get; set; }

    [Column("CPORCENTAJEIMPUESTO3")]
    public double CPORCENTAJEIMPUESTO3 { get; set; }

    [Column("CRETENCION1")]
    public double CRETENCION1 { get; set; }

    [Column("CPORCENTAJERETENCION1")]
    public double CPORCENTAJERETENCION1 { get; set; }

    [Column("CRETENCION2")]
    public double CRETENCION2 { get; set; }

    [Column("CPORCENTAJERETENCION2")]
    public double CPORCENTAJERETENCION2 { get; set; }

    [Column("CDESCUENTO1")]
    public double CDESCUENTO1 { get; set; }

    [Column("CPORCENTAJEDESCUENTO1")]
    public double CPORCENTAJEDESCUENTO1 { get; set; }

    [Column("CDESCUENTO2")]
    public double CDESCUENTO2 { get; set; }

    [Column("CPORCENTAJEDESCUENTO2")]
    public double CPORCENTAJEDESCUENTO2 { get; set; }

    [Column("CDESCUENTO3")]
    public double CDESCUENTO3 { get; set; }

    [Column("CPORCENTAJEDESCUENTO3")]
    public double CPORCENTAJEDESCUENTO3 { get; set; }

    [Column("CDESCUENTO4")]
    public double CDESCUENTO4 { get; set; }

    [Column("CPORCENTAJEDESCUENTO4")]
    public double CPORCENTAJEDESCUENTO4 { get; set; }

    [Column("CDESCUENTO5")]
    public double CDESCUENTO5 { get; set; }

    [Column("CPORCENTAJEDESCUENTO5")]
    public double CPORCENTAJEDESCUENTO5 { get; set; }

    [Column("CTOTAL")]
    public double CTOTAL { get; set; }

    [Column("CPORCENTAJECOMISION")]
    public double CPORCENTAJECOMISION { get; set; }

    [Column("CREFERENCIA")]
    public string? CREFERENCIA { get; set; }

    [Column("COBSERVAMOV")]
    public string? COBSERVAMOV { get; set; }

    [Column("CAFECTAEXISTENCIA")]
    public int CAFECTAEXISTENCIA { get; set; }

    [Column("CAFECTADOSALDOS")]
    public int CAFECTADOSALDOS { get; set; }

    [Column("CAFECTADOINVENTARIO")]
    public int CAFECTADOINVENTARIO { get; set; }

    [Column("CFECHA")]
    public DateTime? CFECHA { get; set; }

    [Column("CMOVTOOCULTO")]
    public int CMOVTOOCULTO { get; set; }

    [Column("CIDMOVTOOWNER")]
    public int CIDMOVTOOWNER { get; set; }

    [Column("CIDMOVTOORIGEN")]
    public int CIDMOVTOORIGEN { get; set; }

    [Column("CUNIDADESPENDIENTES")]
    public double CUNIDADESPENDIENTES { get; set; }

    [Column("CUNIDADESNCPENDIENTES")]
    public double CUNIDADESNCPENDIENTES { get; set; }

    [Column("CUNIDADESORIGEN")]
    public double CUNIDADESORIGEN { get; set; }

    [Column("CUNIDADESNCORIGEN")]
    public double CUNIDADESNCORIGEN { get; set; }

    [Column("CTIPOTRASPASO")]
    public int CTIPOTRASPASO { get; set; }

    [Column("CIDVALORCLASIFICACION")]
    public int CIDVALORCLASIFICACION { get; set; }

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

    [Column("CTIMESTAMP")]
    public string? CTIMESTAMP { get; set; }

    [Column("CGTOMOVTO")]
    public double CGTOMOVTO { get; set; }

    [Column("CSCMOVTO")]
    public string? CSCMOVTO { get; set; }

    [Column("CCOMVENTA")]
    public double CCOMVENTA { get; set; }

    [Column("CIDMOVTODESTINO")]
    public int CIDMOVTODESTINO { get; set; }

    [Column("CNUMEROCONSOLIDACIONES")]
    public int CNUMEROCONSOLIDACIONES { get; set; }

    [Column("COBJIMPU01")]
    public string? COBJIMPU01 { get; set; }

    [Column("CCONFIMP1")]
    public int CCONFIMP1 { get; set; }

    [Column("CCONFIMP2")]
    public int CCONFIMP2 { get; set; }

    [Column("CCONFIMP3")]
    public int CCONFIMP3 { get; set; }

    [Column("CCONFIMP4")]
    public int CCONFIMP4 { get; set; }


}
