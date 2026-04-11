using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppComercial.Domain.Entities;

[Table("admDocumentos")]
public class AdmDocumentos
{
    [Key]
    public int CIDDOCUMENTO { get; set; }

    [Column("CIDDOCUMENTODE")]
    public int CIDDOCUMENTODE { get; set; }

    [Column("CIDCONCEPTODOCUMENTO")]
    public int CIDCONCEPTODOCUMENTO { get; set; }

    [Column("CSERIEDOCUMENTO")]
    public string? CSERIEDOCUMENTO { get; set; }

    [Column("CFOLIO")]
    public double CFOLIO { get; set; }

    [Column("CFECHA")]
    public DateTime? CFECHA { get; set; }

    [Column("CIDCLIENTEPROVEEDOR")]
    public int CIDCLIENTEPROVEEDOR { get; set; }

    [Column("CRAZONSOCIAL")]
    public string? CRAZONSOCIAL { get; set; }

    [Column("CRFC")]
    public string? CRFC { get; set; }

    [Column("CIDAGENTE")]
    public int CIDAGENTE { get; set; }

    [Column("CFECHAVENCIMIENTO")]
    public DateTime? CFECHAVENCIMIENTO { get; set; }

    [Column("CFECHAPRONTOPAGO")]
    public DateTime? CFECHAPRONTOPAGO { get; set; }

    [Column("CFECHAENTREGARECEPCION")]
    public DateTime? CFECHAENTREGARECEPCION { get; set; }

    [Column("CFECHAULTIMOINTERES")]
    public DateTime? CFECHAULTIMOINTERES { get; set; }

    [Column("CIDMONEDA")]
    public int CIDMONEDA { get; set; }

    [Column("CTIPOCAMBIO")]
    public double CTIPOCAMBIO { get; set; }

    [Column("CREFERENCIA")]
    public string? CREFERENCIA { get; set; }

    [Column("COBSERVACIONES")]
    public string? COBSERVACIONES { get; set; }

    [Column("CNATURALEZA")]
    public int CNATURALEZA { get; set; }

    [Column("CIDDOCUMENTOORIGEN")]
    public int CIDDOCUMENTOORIGEN { get; set; }

    [Column("CPLANTILLA")]
    public int CPLANTILLA { get; set; }

    [Column("CUSACLIENTE")]
    public int CUSACLIENTE { get; set; }

    [Column("CUSAPROVEEDOR")]
    public int CUSAPROVEEDOR { get; set; }

    [Column("CAFECTADO")]
    public int CAFECTADO { get; set; }

    [Column("CIMPRESO")]
    public int CIMPRESO { get; set; }

    [Column("CCANCELADO")]
    public int CCANCELADO { get; set; }

    [Column("CDEVUELTO")]
    public int CDEVUELTO { get; set; }

    [Column("CIDPREPOLIZA")]
    public int CIDPREPOLIZA { get; set; }

    [Column("CIDPREPOLIZACANCELACION")]
    public int CIDPREPOLIZACANCELACION { get; set; }

    [Column("CESTADOCONTABLE")]
    public int CESTADOCONTABLE { get; set; }

    [Column("CNETO")]
    public double CNETO { get; set; }

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

    [Column("CDESCUENTOMOV")]
    public double CDESCUENTOMOV { get; set; }

    [Column("CDESCUENTODOC1")]
    public double CDESCUENTODOC1 { get; set; }

    [Column("CDESCUENTODOC2")]
    public double CDESCUENTODOC2 { get; set; }

    [Column("CGASTO1")]
    public double CGASTO1 { get; set; }

    [Column("CGASTO2")]
    public double CGASTO2 { get; set; }

    [Column("CGASTO3")]
    public double CGASTO3 { get; set; }

    [Column("CTOTAL")]
    public double CTOTAL { get; set; }

    [Column("CPENDIENTE")]
    public double CPENDIENTE { get; set; }

    [Column("CTOTALUNIDADES")]
    public double CTOTALUNIDADES { get; set; }

    [Column("CDESCUENTOPRONTOPAGO")]
    public double CDESCUENTOPRONTOPAGO { get; set; }

    [Column("CPORCENTAJEIMPUESTO1")]
    public double CPORCENTAJEIMPUESTO1 { get; set; }

    [Column("CPORCENTAJEIMPUESTO2")]
    public double CPORCENTAJEIMPUESTO2 { get; set; }

    [Column("CPORCENTAJEIMPUESTO3")]
    public double CPORCENTAJEIMPUESTO3 { get; set; }

    [Column("CPORCENTAJERETENCION1")]
    public double CPORCENTAJERETENCION1 { get; set; }

    [Column("CPORCENTAJERETENCION2")]
    public double CPORCENTAJERETENCION2 { get; set; }

    [Column("CPORCENTAJEINTERES")]
    public double CPORCENTAJEINTERES { get; set; }

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

    [Column("CDESTINATARIO")]
    public string? CDESTINATARIO { get; set; }

    [Column("CNUMEROGUIA")]
    public string? CNUMEROGUIA { get; set; }

    [Column("CMENSAJERIA")]
    public string? CMENSAJERIA { get; set; }

    [Column("CCUENTAMENSAJERIA")]
    public string? CCUENTAMENSAJERIA { get; set; }

    [Column("CNUMEROCAJAS")]
    public double CNUMEROCAJAS { get; set; }

    [Column("CPESO")]
    public double CPESO { get; set; }

    [Column("CBANOBSERVACIONES")]
    public int CBANOBSERVACIONES { get; set; }

    [Column("CBANDATOSENVIO")]
    public int CBANDATOSENVIO { get; set; }

    [Column("CBANCONDICIONESCREDITO")]
    public int CBANCONDICIONESCREDITO { get; set; }

    [Column("CBANGASTOS")]
    public int CBANGASTOS { get; set; }

    [Column("CUNIDADESPENDIENTES")]
    public double CUNIDADESPENDIENTES { get; set; }

    [Column("CTIMESTAMP")]
    public string? CTIMESTAMP { get; set; }

    [Column("CIMPCHEQPAQ")]
    public double CIMPCHEQPAQ { get; set; }

    [Column("CSISTORIG")]
    public int CSISTORIG { get; set; }

    [Column("CIDMONEDCA")]
    public int CIDMONEDCA { get; set; }

    [Column("CTIPOCAMCA")]
    public double CTIPOCAMCA { get; set; }

    [Column("CESCFD")]
    public int CESCFD { get; set; }

    [Column("CTIENECFD")]
    public int CTIENECFD { get; set; }

    [Column("CLUGAREXPE")]
    public string? CLUGAREXPE { get; set; }

    [Column("CMETODOPAG")]
    public string? CMETODOPAG { get; set; }

    [Column("CNUMPARCIA")]
    public int CNUMPARCIA { get; set; }

    [Column("CCANTPARCI")]
    public int CCANTPARCI { get; set; }

    [Column("CCONDIPAGO")]
    public string? CCONDIPAGO { get; set; }

    [Column("CNUMCTAPAG")]
    public string? CNUMCTAPAG { get; set; }

    [Column("CGUIDDOCUMENTO")]
    public string? CGUIDDOCUMENTO { get; set; }

    [Column("CUSUARIO")]
    public string? CUSUARIO { get; set; }

    [Column("CIDPROYECTO")]
    public int CIDPROYECTO { get; set; }

    [Column("CIDCUENTA")]
    public int CIDCUENTA { get; set; }

    [Column("CTRANSACTIONID")]
    public string? CTRANSACTIONID { get; set; }

    [Column("CIDCOPIADE")]
    public int CIDCOPIADE { get; set; }

    [Column("CVERESQUE")]
    public string? CVERESQUE { get; set; }

    [Column("CDATOSADICIONALES")]
    public string? CDATOSADICIONALES { get; set; }

    [Column("CIDAPERTURA")]
    public int CIDAPERTURA { get; set; }

    [NotMapped]
    [System.Text.Json.Serialization.JsonPropertyName("ccodigoconcepto")]
    public string? CCODIGOCONCEPTO { get; set; }

    [NotMapped]
    [System.Text.Json.Serialization.JsonPropertyName("cidalmacen")]
    public int? CIDALMACEN { get; set; }
}
