using MediatR;
using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Interfaces.SdkModels;
using System.ComponentModel.DataAnnotations;

namespace AppComercial.Application.Features.Facturas;

/// <summary>
/// Crea una Factura Global CFDI 4.0 para el "Público en General" (RFC XAXX010101000).
/// Cada ticket de POS se registra como partida(s) en el documento.
///
/// MANEJO DE IVA MIXTO:
/// Cuando un ticket tiene productos gravados (16%) y productos exentos/tasa-0 (0%),
/// se crean DOS movimientos por ticket: uno para cada base imponible.
/// El SDK calcula los impuestos basándose en el producto del catálogo de CONTPAQi.
/// Por eso se requieren DOS códigos de producto genérico en el comando:
///   - CodigoProductoGravado (ej. "VENTA-IVA16") → configurado con 16% en CONTPAQi
///   - CodigoProductoExento  (ej. "VENTA-IVA0")  → configurado con 0%  en CONTPAQi
/// </summary>
public class CreateFacturaGlobalCommand : IRequest<CreateFacturaResult>
{
    /// <summary>Código del concepto de Factura Global en CONTPAQi (ej. "FGIH").</summary>
    [Required] public string CodigoConcepto { get; set; } = string.Empty;

    /// <summary>Serie del documento (ej. "FGIH").</summary>
    [StringLength(10)] public string Serie { get; set; } = string.Empty;

    /// <summary>Código del cliente "Público en General" en CONTPAQi (RFC XAXX010101000).</summary>
    [Required] public string CodigoClientePublicoGeneral { get; set; } = "PUBLICOGENERAL";

    // ─── CFDI 4.0 InformacionGlobal ────────────────────────────────────────
    /// <summary>Periodicidad: 01=Diario, 02=Semanal, 03=Quincenal, 04=Mensual, 05=Bimestral.</summary>
    public string Periodicidad { get; set; } = "01";
    /// <summary>Mes al que corresponde la factura global (01-12 o 13-18 para bimestres).</summary>
    public string Meses { get; set; } = "";
    /// <summary>Año al que corresponde la factura global (ej. 2026).</summary>
    public string Anio { get; set; } = "";

    // ─── CFDI 4.0 ──────────────────────────────────────────────────────────
    public string UsoCfdi { get; set; } = "S01";   // Sin efectos fiscales
    public string MetodoPago { get; set; } = "PUE";
    public string FormaPago { get; set; } = "01";

    // ─── Productos genéricos para las dos tasas de IVA ─────────────────────
    /// <summary>
    /// Código del producto en CONTPAQi configurado con ClaveProdServ=01010101 y ClaveUnidad=ACT
    /// y tasa de IVA del 16%. Se usa para las partidas gravadas de cada ticket.
    /// Ejemplo: "GENIVA16"
    /// </summary>
    [Required] public string CodigoProductoGravado { get; set; } = string.Empty;

    /// <summary>
    /// Código del producto en CONTPAQi configurado con ClaveProdServ=01010101 y ClaveUnidad=ACT
    /// y tasa de IVA del 0% (exento/tasa-cero). Se usa cuando el ticket tiene base exenta.
    /// Ejemplo: "GENIVA0". Si todos los tickets son al 16%, puede quedar vacío.
    /// </summary>
    public string CodigoProductoExento { get; set; } = string.Empty;

    /// <summary>Contraseña del CSD para el timbrado desatendido.</summary>
    [Required] public string CsdPassword { get; set; } = string.Empty;
    public bool AutoTimbrar { get; set; } = true;

    /// <summary>Lista de tickets a agrupar (1 ticket = 1 o 2 partidas en la factura global).</summary>
    [MinLength(1)] public List<TicketGlobal> Tickets { get; set; } = new();
}

/// <summary>
/// Representa un Ticket del POS. Puede contener productos gravados al 16% y/o exentos/tasa-0.
/// El handler creará automáticamente dos partidas cuando ambas bases sean mayores a 0.
/// 
/// Ejemplo para el ticket AH105597 con importe total $169.51:
///   FolioTicket        = "AH105597"
///   BaseGravada16      = 39.69    ← el SDK genera Traslado TasaOCuota=0.16 Importe=6.35
///   BaseExenta         = 129.85   ← el SDK genera Traslado TasaOCuota=0.00 Importe=0.00
///   (ImporteTotal no se necesita; se deriva de la suma de bases + IVA calculado por el SDK)
/// </summary>
public class TicketGlobal
{
    /// <summary>Folio del ticket (se usará como NoIdentificacion). Ej. "AH105597".</summary>
    public string FolioTicket { get; set; } = string.Empty;

    /// <summary>
    /// Base imponible del ticket sujeta al 16% de IVA (ValorUnitario para el movimiento gravado).
    /// Si el ticket no tiene productos gravados, dejar en 0.
    /// </summary>
    public double BaseGravada16 { get; set; }

    /// <summary>
    /// Base imponible del ticket sujeta al 0% de IVA (o exenta de IVA).
    /// Si el ticket no tiene productos exentos/tasa-0, dejar en 0.
    /// </summary>
    public double BaseExenta { get; set; }
}

public class CreateFacturaGlobalCommandHandler : IRequestHandler<CreateFacturaGlobalCommand, CreateFacturaResult>
{
    private readonly IContpaqiSdk _sdk;

    public CreateFacturaGlobalCommandHandler(IContpaqiSdk sdk) => _sdk = sdk;

    public async Task<CreateFacturaResult> Handle(CreateFacturaGlobalCommand request, CancellationToken cancellationToken)
    {
        var ahora = DateTime.Now;
        var anio  = string.IsNullOrWhiteSpace(request.Anio)  ? ahora.Year.ToString()    : request.Anio;
        var meses = string.IsNullOrWhiteSpace(request.Meses) ? ahora.Month.ToString("D2") : request.Meses;

        // 1. Crear documento cabecera
        var documento = new tDocumento
        {
            aCodConcepto   = request.CodigoConcepto,
            aSerie         = request.Serie,
            aCodigoCteProv = request.CodigoClientePublicoGeneral,
            aFecha         = ahora.ToString("MM/dd/yyyy"),
            aNumMoneda     = 1,
            aTipoCambio    = 1.0
        };

        var idDocumento = await _sdk.CrearDocumentoAsync(documento);

        // 2. Setear campos CFDI 4.0 incluyendo InformacionGlobal
        var campos = new Dictionary<string, string>
        {
            ["USOCFDI"]      = request.UsoCfdi,
            ["METODOPAGO"]   = request.MetodoPago,
            ["FORMAPAGO"]    = request.FormaPago,
            ["PERIODICIDAD"] = request.Periodicidad,
            ["MESES"]        = meses,
            ["ANIO"]         = anio,
        };
        await _sdk.ActualizarDocumentoPorIdAsync(idDocumento, campos);

        // 3. Agregar partidas — 1 o 2 por ticket según el desglose de IVA
        //
        // Lógica:
        //   • Si BaseGravada16 > 0  → 1 movimiento con CodigoProductoGravado  (SDK aplica 16%)
        //   • Si BaseExenta    > 0  → 1 movimiento con CodigoProductoExento   (SDK aplica 0%)
        //   • Si ambas > 0         → 2 movimientos, ambos con folio del ticket como referencia
        //
        // El SDK de CONTPAQi calcula automáticamente:
        //   Base, TipoFactor, TasaOCuota, Importe
        // según la tasa de impuesto configurada en cada producto del catálogo.

        int consecutivo = 1;
        foreach (var ticket in request.Tickets)
        {
            if (ticket.BaseGravada16 > 0)
            {
                await _sdk.CrearMovimientoAsync(idDocumento, new tMovimiento
                {
                    aConsecutivo      = consecutivo++,
                    aCodProdSer       = request.CodigoProductoGravado,
                    aUnidades         = 1.0,
                    aPrecio           = ticket.BaseGravada16,
                    aCosto            = 0,
                    aCodAlmacen       = string.Empty,
                    aReferencia       = ticket.FolioTicket, // → NoIdentificacion en el XML
                    aCodClasificacion = string.Empty
                });
            }

            if (ticket.BaseExenta > 0)
            {
                var codigoExento = string.IsNullOrWhiteSpace(request.CodigoProductoExento)
                    ? request.CodigoProductoGravado  // Fallback al mismo producto
                    : request.CodigoProductoExento;

                await _sdk.CrearMovimientoAsync(idDocumento, new tMovimiento
                {
                    aConsecutivo      = consecutivo++,
                    aCodProdSer       = codigoExento,
                    aUnidades         = 1.0,
                    aPrecio           = ticket.BaseExenta,
                    aCosto            = 0,
                    aCodAlmacen       = string.Empty,
                    aReferencia       = ticket.FolioTicket,
                    aCodClasificacion = string.Empty
                });
            }

            // Caso donde el ticket solo tenga un importe total sin desglose (legado)
            if (ticket.BaseGravada16 == 0 && ticket.BaseExenta == 0)
            {
                throw new InvalidOperationException(
                    $"El ticket '{ticket.FolioTicket}' debe tener BaseGravada16 y/o BaseExenta > 0. " +
                    "El desglose de bases es obligatorio para la correcta desglosación de IVA en la Factura Global.");
            }
        }

        // 4. Timbrar desatendido
        bool timbrado = false;
        if (request.AutoTimbrar)
        {
            await _sdk.EmitirDocumentoAsync(
                request.CodigoConcepto,
                request.Serie,
                idDocumento,
                request.CsdPassword,
                string.Empty);
            timbrado = true;
        }

        return new CreateFacturaResult
        {
            IdDocumento = idDocumento,
            Timbrado    = timbrado,
            Mensaje     = timbrado ? "Factura Global timbrada exitosamente." : "Factura Global creada sin timbrar."
        };
    }
}
