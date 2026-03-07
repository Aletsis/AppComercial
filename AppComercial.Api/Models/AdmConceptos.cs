using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppComercial.Api.Models;

[Table("admConceptos")]
public class AdmConceptos
{
    [Key]
    public int CIDCONCEPTODOCUMENTO { get; set; }

    [Column("CCODIGOCONCEPTO")]
    public string? CCODIGOCONCEPTO { get; set; }

    [Column("CNOMBRECONCEPTO")]
    public string? CNOMBRECONCEPTO { get; set; }

    [Column("CIDDOCUMENTODE")]
    public int CIDDOCUMENTODE { get; set; }

    [Column("CNATURALEZA")]
    public int CNATURALEZA { get; set; }

    [Column("CDOCTOACREDITO")]
    public int CDOCTOACREDITO { get; set; }

    [Column("CTIPOFOLIO")]
    public int CTIPOFOLIO { get; set; }

    [Column("CMAXIMOMOVTOS")]
    public int CMAXIMOMOVTOS { get; set; }

    [Column("CCREACLIENTE")]
    public int CCREACLIENTE { get; set; }

    [Column("CSUMARPROMOCIONES")]
    public int CSUMARPROMOCIONES { get; set; }

    [Column("CFORMAPREIMPRESA")]
    public string? CFORMAPREIMPRESA { get; set; }

    [Column("CORDENCALCULO")]
    public int CORDENCALCULO { get; set; }

    [Column("CUSANOMBRECTEPROV")]
    public int CUSANOMBRECTEPROV { get; set; }

    [Column("CUSARFC")]
    public int CUSARFC { get; set; }

    [Column("CUSAFECHAVENCIMIENTO")]
    public int CUSAFECHAVENCIMIENTO { get; set; }

    [Column("CUSAFECHAENTREGARECEPCION")]
    public int CUSAFECHAENTREGARECEPCION { get; set; }

    [Column("CUSAMONEDA")]
    public int CUSAMONEDA { get; set; }

    [Column("CUSATIPOCAMBIO")]
    public int CUSATIPOCAMBIO { get; set; }

    [Column("CUSACODIGOAGENTE")]
    public int CUSACODIGOAGENTE { get; set; }

    [Column("CUSANOMBREAGENTE")]
    public int CUSANOMBREAGENTE { get; set; }

    [Column("CUSADIRECCION")]
    public int CUSADIRECCION { get; set; }

    [Column("CUSAREFERENCIA")]
    public int CUSAREFERENCIA { get; set; }

    [Column("CSERIEPOROMISION")]
    public string? CSERIEPOROMISION { get; set; }

    [Column("CANCHOCODIGOPRODUCTO")]
    public int CANCHOCODIGOPRODUCTO { get; set; }

    [Column("CUSANOMBREPRODUCTO")]
    public int CUSANOMBREPRODUCTO { get; set; }

    [Column("CANCHONOMBREPRODUCTO")]
    public int CANCHONOMBREPRODUCTO { get; set; }

    [Column("CUSAALMACEN")]
    public int CUSAALMACEN { get; set; }

    [Column("CANCHOCODIGOALMACEN")]
    public int CANCHOCODIGOALMACEN { get; set; }

    [Column("CANCHOIMPORTES")]
    public int CANCHOIMPORTES { get; set; }

    [Column("CANCHOPORCENTAJES")]
    public int CANCHOPORCENTAJES { get; set; }

    [Column("CANCHOUNIDADPESOMEDIDA")]
    public int CANCHOUNIDADPESOMEDIDA { get; set; }

    [Column("CUSAPRECIO")]
    public int CUSAPRECIO { get; set; }

    [Column("CIDFORMULAPRECIO")]
    public int CIDFORMULAPRECIO { get; set; }

    [Column("CUSACOSTOCAPTURADO")]
    public int CUSACOSTOCAPTURADO { get; set; }

    [Column("CIDFORMULACOSTOCAPTURADO")]
    public int CIDFORMULACOSTOCAPTURADO { get; set; }

    [Column("CUSAEXISTENCIA")]
    public int CUSAEXISTENCIA { get; set; }

    [Column("CUSANETO")]
    public int CUSANETO { get; set; }

    [Column("CIDFORMULANETO")]
    public int CIDFORMULANETO { get; set; }

    [Column("CUSAPORCENTAJEIMPUESTO1")]
    public int CUSAPORCENTAJEIMPUESTO1 { get; set; }

    [Column("CIDFORMULAPORCIMPUESTO1")]
    public int CIDFORMULAPORCIMPUESTO1 { get; set; }

    [Column("CUSAIMPUESTO1")]
    public int CUSAIMPUESTO1 { get; set; }

    [Column("CIDFORMULAIMPUESTO1")]
    public int CIDFORMULAIMPUESTO1 { get; set; }

    [Column("CUSAPORCENTAJEIMPUESTO2")]
    public int CUSAPORCENTAJEIMPUESTO2 { get; set; }

    [Column("CIDFORMULAPORCIMPUESTO2")]
    public int CIDFORMULAPORCIMPUESTO2 { get; set; }

    [Column("CUSAIMPUESTO2")]
    public int CUSAIMPUESTO2 { get; set; }

    [Column("CIDFORMULAIMPUESTO2")]
    public int CIDFORMULAIMPUESTO2 { get; set; }

    [Column("CUSAPORCENTAJEIMPUESTO3")]
    public int CUSAPORCENTAJEIMPUESTO3 { get; set; }

    [Column("CIDFORMULAPORCIMPUESTO3")]
    public int CIDFORMULAPORCIMPUESTO3 { get; set; }

    [Column("CUSAIMPUESTO3")]
    public int CUSAIMPUESTO3 { get; set; }

    [Column("CIDFORMULAIMPUESTO3")]
    public int CIDFORMULAIMPUESTO3 { get; set; }

    [Column("CUSAPORCENTAJERETENCION1")]
    public int CUSAPORCENTAJERETENCION1 { get; set; }

    [Column("CIDFORMULAPORCRETENCION1")]
    public int CIDFORMULAPORCRETENCION1 { get; set; }

    [Column("CUSARETENCION1")]
    public int CUSARETENCION1 { get; set; }

    [Column("CIDFORMULARETENCION1")]
    public int CIDFORMULARETENCION1 { get; set; }

    [Column("CUSAPORCENTAJERETENCION2")]
    public int CUSAPORCENTAJERETENCION2 { get; set; }

    [Column("CIDFORMULAPORCRETENCION2")]
    public int CIDFORMULAPORCRETENCION2 { get; set; }

    [Column("CUSARETENCION2")]
    public int CUSARETENCION2 { get; set; }

    [Column("CIDFORMULARETENCION2")]
    public int CIDFORMULARETENCION2 { get; set; }

    [Column("CUSAPORCENTAJEDESCUENTO1")]
    public int CUSAPORCENTAJEDESCUENTO1 { get; set; }

    [Column("CIDFORMULAPORCDESCUENTO1")]
    public int CIDFORMULAPORCDESCUENTO1 { get; set; }

    [Column("CUSADESCUENTO1")]
    public int CUSADESCUENTO1 { get; set; }

    [Column("CIDFORMULADESCUENTO1")]
    public int CIDFORMULADESCUENTO1 { get; set; }

    [Column("CUSAPORCENTAJEDESCUENTO2")]
    public int CUSAPORCENTAJEDESCUENTO2 { get; set; }

    [Column("CIDFORMULAPORCDESCUENTO2")]
    public int CIDFORMULAPORCDESCUENTO2 { get; set; }

    [Column("CUSADESCUENTO2")]
    public int CUSADESCUENTO2 { get; set; }

    [Column("CIDFORMULADESCUENTO2")]
    public int CIDFORMULADESCUENTO2 { get; set; }

    [Column("CUSAPORCENTAJEDESCUENTO3")]
    public int CUSAPORCENTAJEDESCUENTO3 { get; set; }

    [Column("CIDFORMULAPORCDESCUENTO3")]
    public int CIDFORMULAPORCDESCUENTO3 { get; set; }

    [Column("CUSADESCUENTO3")]
    public int CUSADESCUENTO3 { get; set; }

    [Column("CIDFORMULADESCUENTO3")]
    public int CIDFORMULADESCUENTO3 { get; set; }

    [Column("CUSAPORCENTAJEDESCUENTO4")]
    public int CUSAPORCENTAJEDESCUENTO4 { get; set; }

    [Column("CIDFORMULAPORCDESCUENTO4")]
    public int CIDFORMULAPORCDESCUENTO4 { get; set; }

    [Column("CUSADESCUENTO4")]
    public int CUSADESCUENTO4 { get; set; }

    [Column("CIDFORMULADESCUENTO4")]
    public int CIDFORMULADESCUENTO4 { get; set; }

    [Column("CUSAPORCENTAJEDESCUENTO5")]
    public int CUSAPORCENTAJEDESCUENTO5 { get; set; }

    [Column("CIDFORMULAPORCDESCUENTO5")]
    public int CIDFORMULAPORCDESCUENTO5 { get; set; }

    [Column("CUSADESCUENTO5")]
    public int CUSADESCUENTO5 { get; set; }

    [Column("CIDFORMULADESCUENTO5")]
    public int CIDFORMULADESCUENTO5 { get; set; }

    [Column("CUSATOTAL")]
    public int CUSATOTAL { get; set; }

    [Column("CANCHOREFERENCIA")]
    public int CANCHOREFERENCIA { get; set; }

    [Column("CUSACLASIFICACIONMOVTO")]
    public int CUSACLASIFICACIONMOVTO { get; set; }

    [Column("CANCHOVALORCLASIFICACION")]
    public int CANCHOVALORCLASIFICACION { get; set; }

    [Column("CIDFORMULATOTAL")]
    public int CIDFORMULATOTAL { get; set; }

    [Column("CUSADESCUENTODOC1")]
    public int CUSADESCUENTODOC1 { get; set; }

    [Column("CIDFORMULADESDOC1")]
    public int CIDFORMULADESDOC1 { get; set; }

    [Column("CUSADESCUENTODOC2")]
    public int CUSADESCUENTODOC2 { get; set; }

    [Column("CIDFORMULADESDOC2")]
    public int CIDFORMULADESDOC2 { get; set; }

    [Column("CUSAGASTO1")]
    public int CUSAGASTO1 { get; set; }

    [Column("CIDFORMULAGASTO1")]
    public int CIDFORMULAGASTO1 { get; set; }

    [Column("CUSAGASTO2")]
    public int CUSAGASTO2 { get; set; }

    [Column("CIDFORMULAGASTO2")]
    public int CIDFORMULAGASTO2 { get; set; }

    [Column("CUSAGASTO3")]
    public int CUSAGASTO3 { get; set; }

    [Column("CIDFORMULAGASTO3")]
    public int CIDFORMULAGASTO3 { get; set; }

    [Column("CUSATEXTOEXTRA1")]
    public int CUSATEXTOEXTRA1 { get; set; }

    [Column("CUSATEXTOEXTRA2")]
    public int CUSATEXTOEXTRA2 { get; set; }

    [Column("CUSATEXTOEXTRA3")]
    public int CUSATEXTOEXTRA3 { get; set; }

    [Column("CANCHOTEXTOEXTRA")]
    public int CANCHOTEXTOEXTRA { get; set; }

    [Column("CUSAFECHAEXTRA")]
    public int CUSAFECHAEXTRA { get; set; }

    [Column("CANCHOFECHAEXTRA")]
    public int CANCHOFECHAEXTRA { get; set; }

    [Column("CUSAIMPORTEEXTRA1")]
    public int CUSAIMPORTEEXTRA1 { get; set; }

    [Column("CIDFORMULAEXTRA1")]
    public int CIDFORMULAEXTRA1 { get; set; }

    [Column("CUSAIMPORTEEXTRA2")]
    public int CUSAIMPORTEEXTRA2 { get; set; }

    [Column("CIDFORMULAEXTRA2")]
    public int CIDFORMULAEXTRA2 { get; set; }

    [Column("CUSAIMPORTEEXTRA3")]
    public int CUSAIMPORTEEXTRA3 { get; set; }

    [Column("CIDFORMULAEXTRA3")]
    public int CIDFORMULAEXTRA3 { get; set; }

    [Column("CUSAIMPORTEEXTRA4")]
    public int CUSAIMPORTEEXTRA4 { get; set; }

    [Column("CIDFORMULAEXTRA4")]
    public int CIDFORMULAEXTRA4 { get; set; }

    [Column("CUSATEXTOEXTRA1DOC")]
    public int CUSATEXTOEXTRA1DOC { get; set; }

    [Column("CUSATEXTOEXTRA2DOC")]
    public int CUSATEXTOEXTRA2DOC { get; set; }

    [Column("CUSATEXTOEXTRA3DOC")]
    public int CUSATEXTOEXTRA3DOC { get; set; }

    [Column("CUSAFECHAEXTRADOC")]
    public int CUSAFECHAEXTRADOC { get; set; }

    [Column("CUSAIMPORTEEXTRA1DOC")]
    public int CUSAIMPORTEEXTRA1DOC { get; set; }

    [Column("CUSAIMPORTEEXTRA2DOC")]
    public int CUSAIMPORTEEXTRA2DOC { get; set; }

    [Column("CUSAIMPORTEEXTRA3DOC")]
    public int CUSAIMPORTEEXTRA3DOC { get; set; }

    [Column("CUSAIMPORTEEXTRA4DOC")]
    public int CUSAIMPORTEEXTRA4DOC { get; set; }

    [Column("CUSAEXTRACOMOGASTO")]
    public int CUSAEXTRACOMOGASTO { get; set; }

    [Column("CUSAOBSERVACIONES")]
    public int CUSAOBSERVACIONES { get; set; }

    [Column("CPRESENTAFISCAL")]
    public int CPRESENTAFISCAL { get; set; }

    [Column("CPRESENTAREFERENCIA")]
    public int CPRESENTAREFERENCIA { get; set; }

    [Column("CPRESENTACONDICIONES")]
    public int CPRESENTACONDICIONES { get; set; }

    [Column("CPRESENTAENVIO")]
    public int CPRESENTAENVIO { get; set; }

    [Column("CPRESENTADETALLE")]
    public int CPRESENTADETALLE { get; set; }

    [Column("CPRESENTAIMPRIMIR")]
    public int CPRESENTAIMPRIMIR { get; set; }

    [Column("CPRESENTAPAGAR")]
    public int CPRESENTAPAGAR { get; set; }

    [Column("CPRESENTASALDAR")]
    public int CPRESENTASALDAR { get; set; }

    [Column("CPRESENTADOCUMENTAR")]
    public int CPRESENTADOCUMENTAR { get; set; }

    [Column("CPRESENTAGASTOSCOMPRA")]
    public int CPRESENTAGASTOSCOMPRA { get; set; }

    [Column("CSEGCONTCONCEPTO")]
    public string? CSEGCONTCONCEPTO { get; set; }

    [Column("CBANENCABEZADO")]
    public int CBANENCABEZADO { get; set; }

    [Column("CBANMOVIMIENTO")]
    public int CBANMOVIMIENTO { get; set; }

    [Column("CBANDESCUENTO")]
    public int CBANDESCUENTO { get; set; }

    [Column("CBANIMPUESTO")]
    public int CBANIMPUESTO { get; set; }

    [Column("CBANACCIONAUTOMATICA")]
    public int CBANACCIONAUTOMATICA { get; set; }

    [Column("CTIMESTAMP")]
    public string? CTIMESTAMP { get; set; }

    [Column("CNOFOLIO")]
    public double CNOFOLIO { get; set; }

    [Column("CIDPROCESOSEGURIDAD")]
    public int CIDPROCESOSEGURIDAD { get; set; }

    [Column("CUSAGTOMOV")]
    public int CUSAGTOMOV { get; set; }

    [Column("CUSASCMOV")]
    public int CUSASCMOV { get; set; }

    [Column("CIDASTOCON")]
    public int CIDASTOCON { get; set; }

    [Column("CSCCPTO2")]
    public string? CSCCPTO2 { get; set; }

    [Column("CSCCPTO3")]
    public string? CSCCPTO3 { get; set; }

    [Column("CSCMOVTO")]
    public string? CSCMOVTO { get; set; }

    [Column("CIDCONAUTO")]
    public int CIDCONAUTO { get; set; }

    [Column("CIDALMASUM")]
    public int CIDALMASUM { get; set; }

    [Column("CUSACOMVTA")]
    public int CUSACOMVTA { get; set; }

    [Column("CIDPRSEG02")]
    public int CIDPRSEG02 { get; set; }

    [Column("CIDPRSEG03")]
    public int CIDPRSEG03 { get; set; }

    [Column("CIDPRSEG04")]
    public int CIDPRSEG04 { get; set; }

    [Column("CIDPRSEG05")]
    public int CIDPRSEG05 { get; set; }

    [Column("CFORMAAJ01")]
    public int CFORMAAJ01 { get; set; }

    [Column("CIDPRSEG06")]
    public int CIDPRSEG06 { get; set; }

    [Column("CAPFORMULA")]
    public int CAPFORMULA { get; set; }

    [Column("CESCFD")]
    public int CESCFD { get; set; }

    [Column("CIDFIRMARL")]
    public int CIDFIRMARL { get; set; }

    [Column("CGDAPASSW")]
    public int CGDAPASSW { get; set; }

    [Column("CEMITEYENT")]
    public int CEMITEYENT { get; set; }

    [Column("CBANCFD")]
    public int CBANCFD { get; set; }

    [Column("CREPIMPCFD")]
    public string? CREPIMPCFD { get; set; }

    [Column("CIDDIRSUCU")]
    public int CIDDIRSUCU { get; set; }

    [Column("CBANDIRSUC")]
    public int CBANDIRSUC { get; set; }

    [Column("CVERFACELE")]
    public int CVERFACELE { get; set; }

    [Column("CCALFECHAS")]
    public int CCALFECHAS { get; set; }

    [Column("CTIPCAMTR1")]
    public int CTIPCAMTR1 { get; set; }

    [Column("CTIPCAMTR2")]
    public int CTIPCAMTR2 { get; set; }

    [Column("CCONSOLIDA")]
    public int CCONSOLIDA { get; set; }

    [Column("CENVIODIG")]
    public int CENVIODIG { get; set; }

    [Column("CBANTRANS")]
    public int CBANTRANS { get; set; }

    [Column("CCONFNOAPR")]
    public int CCONFNOAPR { get; set; }

    [Column("CNOAPROB")]
    public int CNOAPROB { get; set; }

    [Column("CAUTOIMPR")]
    public int CAUTOIMPR { get; set; }

    [Column("CRECIBECFD")]
    public int CRECIBECFD { get; set; }

    [Column("CSISTORIG")]
    public int CSISTORIG { get; set; }

    [Column("CIDCPTODE1")]
    public int CIDCPTODE1 { get; set; }

    [Column("CIDCPTODE2")]
    public int CIDCPTODE2 { get; set; }

    [Column("CIDCPTODE3")]
    public int CIDCPTODE3 { get; set; }

    [Column("CPLAMIGCFD")]
    public string? CPLAMIGCFD { get; set; }

    [Column("CIDPRSEG07")]
    public int CIDPRSEG07 { get; set; }

    [Column("CRESERVADO")]
    public int CRESERVADO { get; set; }

    [Column("CVERREFER")]
    public int CVERREFER { get; set; }

    [Column("CVERDOCORI")]
    public int CVERDOCORI { get; set; }

    [Column("CCBB")]
    public int CCBB { get; set; }

    [Column("CCARTAPOR")]
    public int CCARTAPOR { get; set; }

    [Column("CCOMPDONAT")]
    public int CCOMPDONAT { get; set; }

    [Column("COBSXML")]
    public int COBSXML { get; set; }

    [Column("CRUTAENTREGA")]
    public string? CRUTAENTREGA { get; set; }

    [Column("CPREFIJOCONCEPTO")]
    public string? CPREFIJOCONCEPTO { get; set; }

    [Column("CREGIMFISC")]
    public string? CREGIMFISC { get; set; }

    [Column("CCOMPEDUCA")]
    public int CCOMPEDUCA { get; set; }

    [Column("CMETODOPAG")]
    public string? CMETODOPAG { get; set; }

    [Column("CVERESQUE")]
    public string? CVERESQUE { get; set; }

    [Column("CIDFIRMADSL")]
    public string? CIDFIRMADSL { get; set; }

    [Column("CORDENCAPTURA")]
    public string? CORDENCAPTURA { get; set; }

    [Column("CESTATUS")]
    public int CESTATUS { get; set; }

    [Column("CIDMONEDA")]
    public int CIDMONEDA { get; set; }

    [Column("CIDCUENTA")]
    public int CIDCUENTA { get; set; }

    [Column("CCLAVESAT")]
    public string? CCLAVESAT { get; set; }

    [Column("CIDPRSEG08")]
    public int CIDPRSEG08 { get; set; }

    [Column("CUSAOBJIMP")]
    public int CUSAOBJIMP { get; set; }

    [Column("CCONFIEPS")]
    public int CCONFIEPS { get; set; }


}
