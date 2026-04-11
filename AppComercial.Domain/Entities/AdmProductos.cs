using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppComercial.Domain.Entities;

[Table("admProductos")]
public class AdmProductos
{
    [Key]
    public int CIDPRODUCTO { get; set; }

    [Column("CCODIGOPRODUCTO")]
    public string? CCODIGOPRODUCTO { get; set; }

    [Column("CNOMBREPRODUCTO")]
    public string? CNOMBREPRODUCTO { get; set; }

    [Column("CTIPOPRODUCTO")]
    public int CTIPOPRODUCTO { get; set; }

    [Column("CFECHAALTAPRODUCTO")]
    public DateTime? CFECHAALTAPRODUCTO { get; set; }

    [Column("CCONTROLEXISTENCIA")]
    public int CCONTROLEXISTENCIA { get; set; }

    [Column("CIDFOTOPRODUCTO")]
    public int CIDFOTOPRODUCTO { get; set; }

    [Column("CDESCRIPCIONPRODUCTO")]
    public string? CDESCRIPCIONPRODUCTO { get; set; }

    [Column("CMETODOCOSTEO")]
    public int CMETODOCOSTEO { get; set; }

    [Column("CPESOPRODUCTO")]
    public double CPESOPRODUCTO { get; set; }

    [Column("CCOMVENTAEXCEPPRODUCTO")]
    public double CCOMVENTAEXCEPPRODUCTO { get; set; }

    [Column("CCOMCOBROEXCEPPRODUCTO")]
    public double CCOMCOBROEXCEPPRODUCTO { get; set; }

    [Column("CCOSTOESTANDAR")]
    public double CCOSTOESTANDAR { get; set; }

    [Column("CMARGENUTILIDAD")]
    public double CMARGENUTILIDAD { get; set; }

    [Column("CSTATUSPRODUCTO")]
    public int CSTATUSPRODUCTO { get; set; }

    [Column("CIDUNIDADBASE")]
    public int CIDUNIDADBASE { get; set; }

    [Column("CIDUNIDADNOCONVERTIBLE")]
    public int CIDUNIDADNOCONVERTIBLE { get; set; }

    [Column("CFECHABAJA")]
    public DateTime? CFECHABAJA { get; set; }

    [Column("CIMPUESTO1")]
    public double CIMPUESTO1 { get; set; }

    [Column("CIMPUESTO2")]
    public double CIMPUESTO2 { get; set; }

    [Column("CIMPUESTO3")]
    public double CIMPUESTO3 { get; set; }

    [Column("CRETENCION1")]
    public double CRETENCION1 { get; set; }

    [Column("CRETENCION2")]
    public double CRETENCION2 { get; set; }

    [Column("CIDPADRECARACTERISTICA1")]
    public int CIDPADRECARACTERISTICA1 { get; set; }

    [Column("CIDPADRECARACTERISTICA2")]
    public int CIDPADRECARACTERISTICA2 { get; set; }

    [Column("CIDPADRECARACTERISTICA3")]
    public int CIDPADRECARACTERISTICA3 { get; set; }

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

    [Column("CSEGCONTPRODUCTO1")]
    public string? CSEGCONTPRODUCTO1 { get; set; }

    [Column("CSEGCONTPRODUCTO2")]
    public string? CSEGCONTPRODUCTO2 { get; set; }

    [Column("CSEGCONTPRODUCTO3")]
    public string? CSEGCONTPRODUCTO3 { get; set; }

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

    [Column("CPRECIO1")]
    public double CPRECIO1 { get; set; }

    [Column("CPRECIO2")]
    public double CPRECIO2 { get; set; }

    [Column("CPRECIO3")]
    public double CPRECIO3 { get; set; }

    [Column("CPRECIO4")]
    public double CPRECIO4 { get; set; }

    [Column("CPRECIO5")]
    public double CPRECIO5 { get; set; }

    [Column("CPRECIO6")]
    public double CPRECIO6 { get; set; }

    [Column("CPRECIO7")]
    public double CPRECIO7 { get; set; }

    [Column("CPRECIO8")]
    public double CPRECIO8 { get; set; }

    [Column("CPRECIO9")]
    public double CPRECIO9 { get; set; }

    [Column("CPRECIO10")]
    public double CPRECIO10 { get; set; }

    [Column("CBANUNIDADES")]
    public int CBANUNIDADES { get; set; }

    [Column("CBANCARACTERISTICAS")]
    public int CBANCARACTERISTICAS { get; set; }

    [Column("CBANMETODOCOSTEO")]
    public int CBANMETODOCOSTEO { get; set; }

    [Column("CBANMAXMIN")]
    public int CBANMAXMIN { get; set; }

    [Column("CBANPRECIO")]
    public int CBANPRECIO { get; set; }

    [Column("CBANIMPUESTO")]
    public int CBANIMPUESTO { get; set; }

    [Column("CBANCODIGOBARRA")]
    public int CBANCODIGOBARRA { get; set; }

    [Column("CBANCOMPONENTE")]
    public int CBANCOMPONENTE { get; set; }

    [Column("CTIMESTAMP")]
    public string? CTIMESTAMP { get; set; }

    [Column("CERRORCOSTO")]
    public int CERRORCOSTO { get; set; }

    [Column("CFECHAERRORCOSTO")]
    public DateTime? CFECHAERRORCOSTO { get; set; }

    [Column("CPRECIOCALCULADO")]
    public double CPRECIOCALCULADO { get; set; }

    [Column("CESTADOPRECIO")]
    public int CESTADOPRECIO { get; set; }

    [Column("CBANUBICACION")]
    public int CBANUBICACION { get; set; }

    [Column("CESEXENTO")]
    public int CESEXENTO { get; set; }

    [Column("CEXISTENCIANEGATIVA")]
    public int CEXISTENCIANEGATIVA { get; set; }

    [Column("CCOSTOEXT1")]
    public double CCOSTOEXT1 { get; set; }

    [Column("CCOSTOEXT2")]
    public double CCOSTOEXT2 { get; set; }

    [Column("CCOSTOEXT3")]
    public double CCOSTOEXT3 { get; set; }

    [Column("CCOSTOEXT4")]
    public double CCOSTOEXT4 { get; set; }

    [Column("CCOSTOEXT5")]
    public double CCOSTOEXT5 { get; set; }

    [Column("CFECCOSEX1")]
    public DateTime? CFECCOSEX1 { get; set; }

    [Column("CFECCOSEX2")]
    public DateTime? CFECCOSEX2 { get; set; }

    [Column("CFECCOSEX3")]
    public DateTime? CFECCOSEX3 { get; set; }

    [Column("CFECCOSEX4")]
    public DateTime? CFECCOSEX4 { get; set; }

    [Column("CFECCOSEX5")]
    public DateTime? CFECCOSEX5 { get; set; }

    [Column("CMONCOSEX1")]
    public int CMONCOSEX1 { get; set; }

    [Column("CMONCOSEX2")]
    public int CMONCOSEX2 { get; set; }

    [Column("CMONCOSEX3")]
    public int CMONCOSEX3 { get; set; }

    [Column("CMONCOSEX4")]
    public int CMONCOSEX4 { get; set; }

    [Column("CMONCOSEX5")]
    public int CMONCOSEX5 { get; set; }

    [Column("CBANCOSEX")]
    public int CBANCOSEX { get; set; }

    [Column("CESCUOTAI2")]
    public int CESCUOTAI2 { get; set; }

    [Column("CESCUOTAI3")]
    public int CESCUOTAI3 { get; set; }

    [Column("CIDUNIDADCOMPRA")]
    public int CIDUNIDADCOMPRA { get; set; }

    [Column("CIDUNIDADVENTA")]
    public int CIDUNIDADVENTA { get; set; }

    [Column("CSUBTIPO")]
    public int CSUBTIPO { get; set; }

    [Column("CCODALTERN")]
    public string? CCODALTERN { get; set; }

    [Column("CNOMALTERN")]
    public string? CNOMALTERN { get; set; }

    [Column("CDESCCORTA")]
    public string? CDESCCORTA { get; set; }

    [Column("CIDMONEDA")]
    public int CIDMONEDA { get; set; }

    [Column("CUSABASCU")]
    public int CUSABASCU { get; set; }

    [Column("CTIPOPAQUE")]
    public int CTIPOPAQUE { get; set; }

    [Column("CPRECSELEC")]
    public int CPRECSELEC { get; set; }

    [Column("CDESGLOSAI2")]
    public int CDESGLOSAI2 { get; set; }

    [Column("CSEGCONTPRODUCTO4")]
    public string? CSEGCONTPRODUCTO4 { get; set; }

    [Column("CSEGCONTPRODUCTO5")]
    public string? CSEGCONTPRODUCTO5 { get; set; }

    [Column("CSEGCONTPRODUCTO6")]
    public string? CSEGCONTPRODUCTO6 { get; set; }

    [Column("CSEGCONTPRODUCTO7")]
    public string? CSEGCONTPRODUCTO7 { get; set; }

    [Column("CCTAPRED")]
    public string? CCTAPRED { get; set; }

    [Column("CNODESCOMP")]
    public int CNODESCOMP { get; set; }

    [Column("CIDUNIXML")]
    public int CIDUNIXML { get; set; }

    [Column("CCLAVESAT")]
    public string? CCLAVESAT { get; set; }

    [Column("CCANTIDADFISCAL")]
    public double CCANTIDADFISCAL { get; set; }

    [Column("CUNIDADDIMENSION")]
    public int CUNIDADDIMENSION { get; set; }

    [Column("CALTO")]
    public double CALTO { get; set; }

    [Column("CLARGO")]
    public double CLARGO { get; set; }

    [Column("CANCHO")]
    public double CANCHO { get; set; }


    // Propiedades de Navegación
    [ForeignKey(nameof(CIDUNIDADBASE))]
    public virtual AdmUnidadesMedidaPeso? UnidadMedidaBase { get; set; }

}
