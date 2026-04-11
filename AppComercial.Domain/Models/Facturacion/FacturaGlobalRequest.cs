using System.Collections.Generic;

namespace AppComercial.Domain.Models.Facturacion;

public class FacturaGlobalRequest
{
    // Cliente genérico RFC XAXX010101000
    public string CodigoConcepto { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public double Folio { get; set; }

    // CFDI 4.0 Propiedades Globales (Periodicidad, Meses, Año)
    public string Periodicidad { get; set; } = "01"; // 01 = Diario
    public string Meses { get; set; } = "01";        // 01 = Enero
    public string Anio { get; set; } = "2024";

    public string MetodoPago { get; set; } = "PUE";
    public string FormaPago { get; set; } = "01";
    public string UsoCfdi { get; set; } = "S01"; // Sin efectos fiscales

    public string CsdPassword { get; set; } = string.Empty;

    // Tickets agrupados (1 ticket = 1 partida)
    public List<TicketParaGlobalDto> Tickets { get; set; } = new();
}

public class TicketParaGlobalDto
{
    public string FolioTicket { get; set; } = string.Empty; // Servirá de NoIdentificacion o Referencia
    public double ImporteSubtotal { get; set; }
    public double ImporteIva { get; set; }
    public double ImporteTotal { get; set; }
}
