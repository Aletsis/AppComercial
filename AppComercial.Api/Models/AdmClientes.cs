using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppComercial.Api.Models;

[Table("admClientes")]
public class AdmClientes
{
    [Key]
    public int CIDCLIENTEPROVEEDOR { get; set; }

    [Column("CCODIGOCLIENTE")]
    public string? CCODIGOCLIENTE { get; set; }

    [Column("CRAZONSOCIAL")]
    public string? CRAZONSOCIAL { get; set; }

    [Column("CFECHAALTA")]
    public DateTime? CFECHAALTA { get; set; }

    [Column("CRFC")]
    public string? CRFC { get; set; }

    [Column("CCURP")]
    public string? CCURP { get; set; }

    [Column("CDENCOMERCIAL")]
    public string? CDENCOMERCIAL { get; set; }

    [Column("CREPLEGAL")]
    public string? CREPLEGAL { get; set; }

    [Column("CIDMONEDA")]
    public int CIDMONEDA { get; set; }

    [Column("CLISTAPRECIOCLIENTE")]
    public int CLISTAPRECIOCLIENTE { get; set; }

    [Column("CDESCUENTODOCTO")]
    public double CDESCUENTODOCTO { get; set; }

    [Column("CDESCUENTOMOVTO")]
    public double CDESCUENTOMOVTO { get; set; }

    [Column("CBANVENTACREDITO")]
    public int CBANVENTACREDITO { get; set; }

    [Column("CIDVALORCLASIFCLIENTE1")]
    public int CIDVALORCLASIFCLIENTE1 { get; set; }

    [Column("CIDVALORCLASIFCLIENTE2")]
    public int CIDVALORCLASIFCLIENTE2 { get; set; }

    [Column("CIDVALORCLASIFCLIENTE3")]
    public int CIDVALORCLASIFCLIENTE3 { get; set; }

    [Column("CIDVALORCLASIFCLIENTE4")]
    public int CIDVALORCLASIFCLIENTE4 { get; set; }

    [Column("CIDVALORCLASIFCLIENTE5")]
    public int CIDVALORCLASIFCLIENTE5 { get; set; }

    [Column("CIDVALORCLASIFCLIENTE6")]
    public int CIDVALORCLASIFCLIENTE6 { get; set; }

    [Column("CTIPOCLIENTE")]
    public int CTIPOCLIENTE { get; set; }

    [Column("CESTATUS")]
    public int CESTATUS { get; set; }

    [Column("CFECHABAJA")]
    public DateTime? CFECHABAJA { get; set; }

    [Column("CFECHAULTIMAREVISION")]
    public DateTime? CFECHAULTIMAREVISION { get; set; }

    [Column("CLIMITECREDITOCLIENTE")]
    public double CLIMITECREDITOCLIENTE { get; set; }

    [Column("CDIASCREDITOCLIENTE")]
    public int CDIASCREDITOCLIENTE { get; set; }

    [Column("CBANEXCEDERCREDITO")]
    public int CBANEXCEDERCREDITO { get; set; }

    [Column("CDESCUENTOPRONTOPAGO")]
    public double CDESCUENTOPRONTOPAGO { get; set; }

    [Column("CDIASPRONTOPAGO")]
    public int CDIASPRONTOPAGO { get; set; }

    [Column("CINTERESMORATORIO")]
    public double CINTERESMORATORIO { get; set; }

    [Column("CDIAPAGO")]
    public int CDIAPAGO { get; set; }

    [Column("CDIASREVISION")]
    public int CDIASREVISION { get; set; }

    [Column("CMENSAJERIA")]
    public string? CMENSAJERIA { get; set; }

    [Column("CCUENTAMENSAJERIA")]
    public string? CCUENTAMENSAJERIA { get; set; }

    [Column("CDIASEMBARQUECLIENTE")]
    public int CDIASEMBARQUECLIENTE { get; set; }

    [Column("CIDALMACEN")]
    public int CIDALMACEN { get; set; }

    [Column("CIDAGENTEVENTA")]
    public int CIDAGENTEVENTA { get; set; }

    [Column("CIDAGENTECOBRO")]
    public int CIDAGENTECOBRO { get; set; }

    [Column("CRESTRICCIONAGENTE")]
    public int CRESTRICCIONAGENTE { get; set; }

    [Column("CIMPUESTO1")]
    public double CIMPUESTO1 { get; set; }

    [Column("CIMPUESTO2")]
    public double CIMPUESTO2 { get; set; }

    [Column("CIMPUESTO3")]
    public double CIMPUESTO3 { get; set; }

    [Column("CRETENCIONCLIENTE1")]
    public double CRETENCIONCLIENTE1 { get; set; }

    [Column("CRETENCIONCLIENTE2")]
    public double CRETENCIONCLIENTE2 { get; set; }

    [Column("CIDVALORCLASIFPROVEEDOR1")]
    public int CIDVALORCLASIFPROVEEDOR1 { get; set; }

    [Column("CIDVALORCLASIFPROVEEDOR2")]
    public int CIDVALORCLASIFPROVEEDOR2 { get; set; }

    [Column("CIDVALORCLASIFPROVEEDOR3")]
    public int CIDVALORCLASIFPROVEEDOR3 { get; set; }

    [Column("CIDVALORCLASIFPROVEEDOR4")]
    public int CIDVALORCLASIFPROVEEDOR4 { get; set; }

    [Column("CIDVALORCLASIFPROVEEDOR5")]
    public int CIDVALORCLASIFPROVEEDOR5 { get; set; }

    [Column("CIDVALORCLASIFPROVEEDOR6")]
    public int CIDVALORCLASIFPROVEEDOR6 { get; set; }

    [Column("CLIMITECREDITOPROVEEDOR")]
    public double CLIMITECREDITOPROVEEDOR { get; set; }

    [Column("CDIASCREDITOPROVEEDOR")]
    public int CDIASCREDITOPROVEEDOR { get; set; }

    [Column("CTIEMPOENTREGA")]
    public int CTIEMPOENTREGA { get; set; }

    [Column("CDIASEMBARQUEPROVEEDOR")]
    public int CDIASEMBARQUEPROVEEDOR { get; set; }

    [Column("CIMPUESTOPROVEEDOR1")]
    public double CIMPUESTOPROVEEDOR1 { get; set; }

    [Column("CIMPUESTOPROVEEDOR2")]
    public double CIMPUESTOPROVEEDOR2 { get; set; }

    [Column("CIMPUESTOPROVEEDOR3")]
    public double CIMPUESTOPROVEEDOR3 { get; set; }

    [Column("CRETENCIONPROVEEDOR1")]
    public double CRETENCIONPROVEEDOR1 { get; set; }

    [Column("CRETENCIONPROVEEDOR2")]
    public double CRETENCIONPROVEEDOR2 { get; set; }

    [Column("CBANINTERESMORATORIO")]
    public int CBANINTERESMORATORIO { get; set; }

    [Column("CCOMVENTAEXCEPCLIENTE")]
    public double CCOMVENTAEXCEPCLIENTE { get; set; }

    [Column("CCOMCOBROEXCEPCLIENTE")]
    public double CCOMCOBROEXCEPCLIENTE { get; set; }

    [Column("CBANPRODUCTOCONSIGNACION")]
    public int CBANPRODUCTOCONSIGNACION { get; set; }

    [Column("CSEGCONTCLIENTE1")]
    public string? CSEGCONTCLIENTE1 { get; set; }

    [Column("CSEGCONTCLIENTE2")]
    public string? CSEGCONTCLIENTE2 { get; set; }

    [Column("CSEGCONTCLIENTE3")]
    public string? CSEGCONTCLIENTE3 { get; set; }

    [Column("CSEGCONTCLIENTE4")]
    public string? CSEGCONTCLIENTE4 { get; set; }

    [Column("CSEGCONTCLIENTE5")]
    public string? CSEGCONTCLIENTE5 { get; set; }

    [Column("CSEGCONTCLIENTE6")]
    public string? CSEGCONTCLIENTE6 { get; set; }

    [Column("CSEGCONTCLIENTE7")]
    public string? CSEGCONTCLIENTE7 { get; set; }

    [Column("CSEGCONTPROVEEDOR1")]
    public string? CSEGCONTPROVEEDOR1 { get; set; }

    [Column("CSEGCONTPROVEEDOR2")]
    public string? CSEGCONTPROVEEDOR2 { get; set; }

    [Column("CSEGCONTPROVEEDOR3")]
    public string? CSEGCONTPROVEEDOR3 { get; set; }

    [Column("CSEGCONTPROVEEDOR4")]
    public string? CSEGCONTPROVEEDOR4 { get; set; }

    [Column("CSEGCONTPROVEEDOR5")]
    public string? CSEGCONTPROVEEDOR5 { get; set; }

    [Column("CSEGCONTPROVEEDOR6")]
    public string? CSEGCONTPROVEEDOR6 { get; set; }

    [Column("CSEGCONTPROVEEDOR7")]
    public string? CSEGCONTPROVEEDOR7 { get; set; }

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

    [Column("CBANCREDITOYCOBRANZA")]
    public int CBANCREDITOYCOBRANZA { get; set; }

    [Column("CBANENVIO")]
    public int CBANENVIO { get; set; }

    [Column("CBANAGENTE")]
    public int CBANAGENTE { get; set; }

    [Column("CBANIMPUESTO")]
    public int CBANIMPUESTO { get; set; }

    [Column("CBANPRECIO")]
    public int CBANPRECIO { get; set; }

    [Column("CTIMESTAMP")]
    public string? CTIMESTAMP { get; set; }

    [Column("CFACTERC01")]
    public int CFACTERC01 { get; set; }

    [Column("CCOMVENTA")]
    public double CCOMVENTA { get; set; }

    [Column("CCOMCOBRO")]
    public double CCOMCOBRO { get; set; }

    [Column("CIDMONEDA2")]
    public int CIDMONEDA2 { get; set; }

    [Column("CEMAIL1")]
    public string? CEMAIL1 { get; set; }

    [Column("CEMAIL2")]
    public string? CEMAIL2 { get; set; }

    [Column("CEMAIL3")]
    public string? CEMAIL3 { get; set; }

    [Column("CTIPOENTRE")]
    public int CTIPOENTRE { get; set; }

    [Column("CCONCTEEMA")]
    public int CCONCTEEMA { get; set; }

    [Column("CFTOADDEND")]
    public int CFTOADDEND { get; set; }

    [Column("CIDCERTCTE")]
    public int CIDCERTCTE { get; set; }

    [Column("CENCRIPENT")]
    public int CENCRIPENT { get; set; }

    [Column("CBANCFD")]
    public int CBANCFD { get; set; }

    [Column("CTEXTOEXTRA4")]
    public string? CTEXTOEXTRA4 { get; set; }

    [Column("CTEXTOEXTRA5")]
    public string? CTEXTOEXTRA5 { get; set; }

    [Column("CIMPORTEEXTRA5")]
    public double CIMPORTEEXTRA5 { get; set; }

    [Column("CIDADDENDA")]
    public int CIDADDENDA { get; set; }

    [Column("CCODPROVCO")]
    public string? CCODPROVCO { get; set; }

    [Column("CENVACUSE")]
    public int CENVACUSE { get; set; }

    [Column("CCON1NOM")]
    public string? CCON1NOM { get; set; }

    [Column("CCON1TEL")]
    public string? CCON1TEL { get; set; }

    [Column("CQUITABLAN")]
    public int CQUITABLAN { get; set; }

    [Column("CFMTOENTRE")]
    public int CFMTOENTRE { get; set; }

    [Column("CIDCOMPLEM")]
    public int CIDCOMPLEM { get; set; }

    [Column("CDESGLOSAI2")]
    public int CDESGLOSAI2 { get; set; }

    [Column("CLIMDOCTOS")]
    public int CLIMDOCTOS { get; set; }

    [Column("CSITIOFTP")]
    public string? CSITIOFTP { get; set; }

    [Column("CUSRFTP")]
    public string? CUSRFTP { get; set; }

    [Column("CMETODOPAG")]
    public string? CMETODOPAG { get; set; }

    [Column("CNUMCTAPAG")]
    public string? CNUMCTAPAG { get; set; }

    [Column("CIDCUENTA")]
    public int CIDCUENTA { get; set; }

    [Column("CUSOCFDI")]
    public string? CUSOCFDI { get; set; }

    [Column("CREGIMFISC")]
    public string? CREGIMFISC { get; set; }

    [Column("CWHATSAPP")]
    public string? CWHATSAPP { get; set; }

    [Column("CCODIGOALTERNO")]
    public string? CCODIGOALTERNO { get; set; }


}
