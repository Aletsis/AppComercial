namespace AppComercial.Domain.Models.Facturacion;

public class NotaCreditoRequest
{
    public string CodigoCliente { get; set; } = string.Empty;
    public string CodigoConcepto { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public double Folio { get; set; }

    public string MetodoPago { get; set; } = "PUE";
    public string FormaPago { get; set; } = "01"; // Usualmente "Efectivo" o acorde a la operacion
    public string UsoCfdi { get; set; } = "G02"; // Devoluciones, descuentos o bonificaciones

    public string CsdPassword { get; set; } = string.Empty;

    // UUID a relacionar (La factura original a la cual le aplicarán la nota de crédito)
    public string UuidFacturaOrigen { get; set; } = string.Empty;
    public string TipoRelacionSat { get; set; } = "01"; // 01 = Nota de crédito de CFDI

    // Información del adeudo original para saldar la factura en la BD de CONTPAQi.
    // Esto es para que en la CXC, el saldo de la factura disminuya el importe de esta NC.
    public string ConceptoFacturaOrigen { get; set; } = string.Empty;
    public string SerieFacturaOrigen { get; set; } = string.Empty;
    public double FolioFacturaOrigen { get; set; }

    public string CodigoProductoDevolucion { get; set; } = string.Empty; // Producto genérico
    public double ImporteTotalNota { get; set; }
}

public class PagoGenericoRequest
{
    public string CodigoConceptoPago { get; set; } = string.Empty;
    public string SeriePago { get; set; } = string.Empty;
    public double FolioPago { get; set; }
    
    // Factura que recibe el pago
    public string CodigoConceptoFactura { get; set; } = string.Empty;
    public string SerieFactura { get; set; } = string.Empty;
    public double FolioFactura { get; set; }

    // Importe a pagar
    public double ImporteAbono { get; set; }
}
