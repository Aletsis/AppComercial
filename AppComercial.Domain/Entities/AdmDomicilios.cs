using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppComercial.Domain.Entities;

[Table("admDomicilios")]
public class AdmDomicilios
{
    [Key]
    [Column("CIDDIRECCION")]
    public int Id { get; set; }

    [Column("CIDCATALOGO")]
    public int CatalogoId { get; set; } // Puede ser Id del Cliente, Proveedor, Empresa

    [Column("CTIPOCATALOGO")]
    public int TipoCatalogo { get; set; } // 1=Clientes, 2=Proveedores, etc.

    [Column("CTIPODIRECCION")]
    public int TipoDireccion { get; set; } // 0=Fiscal, 1=Envio

    [Column("CNOMBRECALLE")]
    public string Calle { get; set; } = string.Empty;

    [Column("CNUMEROEXTERIOR")]
    public string NumeroExterior { get; set; } = string.Empty;

    [Column("CNUMEROINTERIOR")]
    public string NumeroInterior { get; set; } = string.Empty;

    [Column("CCOLONIA")]
    public string Colonia { get; set; } = string.Empty;

    [Column("CMUNICIPIO")]
    public string Municipio { get; set; } = string.Empty;

    [Column("CCIUDAD")]
    public string Ciudad { get; set; } = string.Empty;

    [Column("CESTADO")]
    public string Estado { get; set; } = string.Empty;

    [Column("CPAIS")]
    public string Pais { get; set; } = string.Empty;

    [Column("CCODIGOPOSTAL")]
    public string CodigoPostal { get; set; } = string.Empty;
}
