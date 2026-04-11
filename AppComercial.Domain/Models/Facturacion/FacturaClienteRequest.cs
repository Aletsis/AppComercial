using System.Collections.Generic;

namespace AppComercial.Domain.Models.Facturacion;

public class FacturaClienteRequest
{
    public string CodigoCliente { get; set; } = string.Empty;
    public string CodigoConcepto { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public double Folio { get; set; }
    
    // CFDI 4.0
    public string MetodoPago { get; set; } = "PUE"; // PUE, PPD
    public string FormaPago { get; set; } = "01";   // 01, 04, 28...
    public string UsoCfdi { get; set; } = "G03";    // Gastos en general

    public string CsdPassword { get; set; } = string.Empty;
    public string CsdEmail { get; set; } = string.Empty;

    public List<FacturaPartidaRequest> Partidas { get; set; } = new();
}

public class FacturaPartidaRequest
{
    public string CodigoProducto { get; set; } = string.Empty;
    public string Referencia { get; set; } = string.Empty;
    public double Cantidad { get; set; }
    public double PrecioUnitario { get; set; }
}
