using MediatR;
using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Interfaces.SdkModels;
using AppComercial.Application.DTOs;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace AppComercial.Application.Features.Facturas;

/// <summary>
/// Crea una Factura Global CFDI 4.0 para el "Público en General" (RFC XAXX010101000).
///
/// MODELO DE CONCEPTOS EXPLÍCITOS (recomendado):
///   Cada nota/ticket se envía como un <see cref="ConceptoGlobal"/> con su lista de
///   <see cref="TrasladoConcepto"/> por TasaOCuota.
///   El handler crea UN SOLO movimiento SDK por concepto (ticket), con precio = suma de
///   todas las bases. El desglose por tasa se pasa vía cObservaMov, que CONTPAQi interpreta
///   para generar múltiples &lt;cfdi:Traslado&gt; dentro del mismo &lt;cfdi:Concepto&gt;.
///   Este comportamiento replica exactamente lo que hace la MacroGeneracionGlobal.txt.
///
/// MODELO LEGACY DE TICKETS (retrocompatibilidad):
///   Si <see cref="Conceptos"/> está vacío pero <see cref="Tickets"/> tiene datos,
///   el handler usa el comportamiento anterior (BaseGravada16 / BaseExenta).
/// </summary>
public class CreateFacturaGlobalCommand : IRequest<CreateFacturaResult>
{
    /// <summary>Código del concepto de Factura Global en CONTPAQi (ej. "FGIH").</summary>
    [Required] public string CodigoConcepto { get; set; } = string.Empty;

    /// <summary>Serie del documento (ej. "FGIS").</summary>
    [StringLength(10)] public string Serie { get; set; } = string.Empty;

    /// <summary>Código del cliente "Público en General" en CONTPAQi (RFC XAXX010101000).</summary>
    [Required] public string CodigoClientePublicoGeneral { get; set; } = "PUBLICOGENERAL";

    // ─── CFDI 4.0 InformacionGlobal ────────────────────────────────────────
    /// <summary>Periodicidad: 01=Diario, 02=Semanal, 03=Quincenal, 04=Mensual, 05=Bimestral.</summary>
    public string Periodicidad { get; set; } = "01";
    /// <summary>Mes al que corresponde la factura global (01-12).</summary>
    public string Meses { get; set; } = "";
    /// <summary>Año al que corresponde la factura global (ej. 2025).</summary>
    public string Anio { get; set; } = "";

    // ─── CFDI 4.0 ──────────────────────────────────────────────────────────
    public string UsoCfdi    { get; set; } = "S01";  // Sin efectos fiscales
    public string MetodoPago { get; set; } = "PUE";
    public string FormaPago  { get; set; } = "01";

    // ─── Productos genéricos del catálogo CONTPAQi por tasa de IVA ─────────
    /// <summary>
    /// Código del producto en CONTPAQi configurado con ClaveProdServ=01010101, ClaveUnidad=ACT
    /// y tasa de IVA del 16%. Requerido cuando algún concepto tiene traslados al 16%.
    /// Ejemplo: "GENIVA16"
    /// </summary>
    public string CodigoProductoGravado { get; set; } = string.Empty;

    /// <summary>
    /// Código del producto en CONTPAQi configurado con ClaveProdServ=01010101, ClaveUnidad=ACT
    /// y tasa de IVA del 0% o exento. Requerido cuando algún concepto tiene traslados al 0%/Exento.
    /// Ejemplo: "GENIVA0". Si todos los conceptos son al 16%, puede quedar vacío.
    /// </summary>
    public string CodigoProductoExento { get; set; } = string.Empty;

    /// <summary>Contraseña del CSD para el timbrado desatendido.</summary>
    public string CsdPassword { get; set; } = string.Empty;
    public bool   AutoTimbrar { get; set; } = true;

    /// <summary>
    /// Código del almacén en CONTPAQi requerido por el SDK al agregar movimientos.
    /// Ejemplo: "1". Consulta tus almacenes en CONTPAQi → Catálogos → Almacenes.
    /// </summary>
    public string CodigoAlmacen { get; set; } = "1";

    // ─── MODELO NUEVO: Conceptos con desglose explícito por tasa ───────────
    /// <summary>
    /// Lista de conceptos (una nota/ticket por elemento).
    /// Si se envía, tiene prioridad sobre <see cref="Tickets"/>.
    /// </summary>
    public List<ConceptoGlobal> Conceptos { get; set; } = new();

    // ─── MODELO LEGACY: Tickets con BaseGravada16 / BaseExenta ─────────────
    /// <summary>
    /// Lista de tickets en el formato anterior (retrocompatibilidad).
    /// Solo se usa si <see cref="Conceptos"/> está vacío.
    /// </summary>
    public List<TicketGlobal> Tickets { get; set; } = new();
}

/// <summary>
/// Representa una nota/ticket del POS como concepto explícito del CFDI Global.
/// ClaveProdServ=01010101, ClaveUnidad=ACT, Descripcion="Venta".
/// </summary>
public class ConceptoGlobal
{
    /// <summary>
    /// Serie + Folio de la nota (NoIdentificacion en el CFDI).
    /// Ejemplo: "CS683705" o "A1234".
    /// </summary>
    public string NoIdentificacion { get; set; } = string.Empty;

    /// <summary>Importe sin IVA de la nota (Subtotal de la venta).</summary>
    public double ValorUnitario { get; set; }

    /// <summary>Igual a ValorUnitario cuando Cantidad=1.</summary>
    public double Importe { get; set; }

    /// <summary>
    /// Traslados de impuesto por tasa distinta.
    /// Una línea para 16%, otra para 0%, y/u otra para Exento.
    /// </summary>
    public List<TrasladoConcepto> Traslados { get; set; } = new();
}

/// <summary>
/// Traslado de IVA dentro de un concepto, con su base y TasaOCuota específica.
/// </summary>
public class TrasladoConcepto
{
    public double Base       { get; set; }
    public string Impuesto   { get; set; } = "002";
    public string TipoFactor { get; set; } = "Tasa";
    /// <summary>Tasa en formato SAT: "0.160000", "0.080000", "0.000000".</summary>
    public string TasaOCuota { get; set; } = "0.160000";
    public double Importe    { get; set; }
}

/// <summary>
/// Modelo legacy de ticket. Se usa cuando Conceptos está vacío.
/// </summary>
public class TicketGlobal
{
    /// <summary>Folio del ticket (se usará como Referencia en el movimiento SDK).</summary>
    public string FolioTicket { get; set; } = string.Empty;
    /// <summary>Base imponible al 16% de IVA.</summary>
    public double BaseGravada16 { get; set; }
    /// <summary>Base imponible al 0% de IVA (o exenta).</summary>
    public double BaseExenta { get; set; }
}

public class CreateFacturaGlobalCommandHandler : IRequestHandler<CreateFacturaGlobalCommand, CreateFacturaResult>
{
    private readonly IContpaqiSdk _sdk;
    private readonly IConfiguration _configuration;

    public CreateFacturaGlobalCommandHandler(IContpaqiSdk sdk, IConfiguration configuration)
    {
        _sdk = sdk;
        _configuration = configuration;
    }

    public async Task<CreateFacturaResult> Handle(CreateFacturaGlobalCommand request, CancellationToken cancellationToken)
    {
        var ahora = DateTime.Now;
        var anio  = string.IsNullOrWhiteSpace(request.Anio)  ? ahora.Year.ToString()     : request.Anio;
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

        // 2. Agregar partidas según el modelo recibido
        int consecutivo = 1;
        bool usaModeloNuevo = request.Conceptos.Count > 0;

        if (usaModeloNuevo)
        {
            // ── MODELO NUEVO: un movimiento SDK por concepto (ticket completo) ──────
            //
            // EQUIVALENCIA CON LA MACRO (MacroGeneracionGlobal.txt):
            //   Macro → lPrecio = baseIVA16 + baseTasa0 + baseExento  (precio total del ticket)
            //   Macro → cObservaMov = "ImpuestoN Tasa X Base Y Total Z\r\n..." (desglose por tasa)
            //   Macro → Un solo movimiento SDK por fila/ticket
            //
            // CONTPAQi interpreta cObservaMov para generar múltiples <cfdi:Traslado> dentro
            // del mismo <cfdi:Concepto>, replicando el XML de referencia (FFGIH0000021977.xml).
            foreach (var concepto in request.Conceptos)
            {
                // Precio total = suma de todas las bases del concepto (sin IVA)
                double precioTotal = concepto.Traslados.Sum(t => t.Base);
                if (precioTotal <= 0) continue;

                // Determinar qué producto genérico usar:
                // Si hay alguna tasa gravada (>0%), usar el producto gravado.
                // Si todos son 0%/Exento, usar el producto exento (fallback: gravado).
                bool tieneGravado = concepto.Traslados.Any(t =>
                    !t.TipoFactor.Equals("Exento", StringComparison.OrdinalIgnoreCase) &&
                    t.TasaOCuota != "0.000000");

                string codProducto = tieneGravado
                    ? request.CodigoProductoGravado
                    : (string.IsNullOrWhiteSpace(request.CodigoProductoExento)
                        ? request.CodigoProductoGravado
                        : request.CodigoProductoExento);

                // Crear el movimiento base con el precio total del ticket (= un <cfdi:Concepto>)
                int idMovimiento = await _sdk.CrearMovimientoAsync(idDocumento, new tMovimiento
                {
                    aConsecutivo      = consecutivo++,
                    aCodProdSer       = codProducto,
                    aUnidades         = 1.0,
                    aPrecio           = precioTotal,
                    aCosto            = 0,
                    aCodAlmacen       = request.CodigoAlmacen,
                    aReferencia       = TruncateRef(concepto.NoIdentificacion),
                    aCodClasificacion = string.Empty
                });

                // Construir cObservaMov con el desglose por tasa (igual que la macro)
                // Formato de cada línea: "ImpuestoN Tasa T Base B Total Im"
                // CONTPAQi parsea este campo para generar los <cfdi:Traslado> correctos.
                if (idMovimiento > 0 && concepto.Traslados.Count > 0)
                {
                    var obsBuilder = new System.Text.StringBuilder();
                    foreach (var traslado in concepto.Traslados)
                    {
                        if (traslado.TipoFactor.Equals("Exento", StringComparison.OrdinalIgnoreCase))
                        {
                            // Exento: sin TasaOCuota numérica, Total siempre 0
                            // Replica macro: "Impuesto2 Tasa Exento Base X.XX Total 0.000000"
                            obsBuilder.AppendLine(
                                $"Impuesto{traslado.Impuesto} Tasa Exento " +
                                $"Base {traslado.Base:F2} " +
                                $"Total 0.000000");
                        }
                        else if (traslado.TasaOCuota == "0.000000")
                        {
                            // Tasa 0%: escribe la tasa con 6 decimales (igual que la macro)
                            // Replica macro: "Impuesto2 Tasa 0.000000 Base X.XX Total 0.000000"
                            obsBuilder.AppendLine(
                                $"Impuesto{traslado.Impuesto} Tasa 0.000000 " +
                                $"Base {traslado.Base:F2} " +
                                $"Total 0.000000");
                        }
                        else
                        {
                            // Tasa gravada (16%, 8%): escribe la tasa como decimal
                            // Replica macro: "Impuesto2 Tasa 0.16 Base X.XX Total Y.YY"
                            double tasaDecimal = double.TryParse(
                                traslado.TasaOCuota,
                                System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.InvariantCulture,
                                out double t) ? t : 0;

                            obsBuilder.AppendLine(
                                $"Impuesto{traslado.Impuesto} Tasa {tasaDecimal} " +
                                $"Base {traslado.Base:F2} " +
                                $"Total {traslado.Importe:F2}");
                        }
                    }

                    // Pasar el desglose al movimiento vía fSetDatoMovimiento("cObservaMov", ...)
                    // Ignorar si falla — el movimiento ya fue creado y el timbrado puede continuar.
                    try
                    {
                        await _sdk.ActualizarMovimientoAsync(idDocumento, idMovimiento, new Dictionary<string, string>
                        {
                            ["cObservaMov"] = obsBuilder.ToString().TrimEnd()
                        });
                    }
                    catch
                    {
                        // cObservaMov mejora la estructura CFDI pero no bloquea la generación.
                        // Si el campo no es soportado en esta versión del SDK, se continúa.
                    }
                }
            }
        }
        else
        {
            // ── MODELO LEGACY: BaseGravada16 / BaseExenta ────────────────────────
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
                        aCodAlmacen       = request.CodigoAlmacen,
                        aReferencia       = TruncateRef(ticket.FolioTicket),
                        aCodClasificacion = string.Empty
                    });
                }

                if (ticket.BaseExenta > 0)
                {
                    var codigoExento = string.IsNullOrWhiteSpace(request.CodigoProductoExento)
                        ? request.CodigoProductoGravado
                        : request.CodigoProductoExento;

                    await _sdk.CrearMovimientoAsync(idDocumento, new tMovimiento
                    {
                        aConsecutivo      = consecutivo++,
                        aCodProdSer       = codigoExento,
                        aUnidades         = 1.0,
                        aPrecio           = ticket.BaseExenta,
                        aCosto            = 0,
                        aCodAlmacen       = request.CodigoAlmacen,
                        aReferencia       = TruncateRef(ticket.FolioTicket),
                        aCodClasificacion = string.Empty
                    });
                }
            }
        }

        // 3. Leer Folio y Serie reales asignados por el SDK
        string folioReal = "0";
        string serieReal = request.Serie;
        try { folioReal = await _sdk.LeerDatoDocumentoAsync("CFOLIO"); }
        catch { try { folioReal = await _sdk.LeerDatoDocumentoAsync("cFolio"); } catch { folioReal = idDocumento.ToString(); } }

        try { serieReal = await _sdk.LeerDatoDocumentoAsync("CSERIEDOCUMENTO"); }
        catch
        {
            try { serieReal = await _sdk.LeerDatoDocumentoAsync("CSERIE"); }
            catch { try { serieReal = await _sdk.LeerDatoDocumentoAsync("cSerie"); } catch { /* mantener serie del request */ } }
        }

        // 4. Configurar campos CFDI 4.0 e InformacionGlobal
        // Según comunidad SDK (Andres Ramos): FormaPago -> CMETODOPAG, MetodoPago -> CCANTPARCI
        var campos = new Dictionary<string, string>
        {
            ["cUsoCFDI"]      = request.UsoCfdi,
            ["CMETODOPAG"]    = request.FormaPago,
            ["CCANTPARCI"]    = request.MetodoPago.ToUpper() == "PPD" ? "2" : "1",
            ["CPERIODICIDAD"] = request.Periodicidad,
            ["CMESES"]        = meses,
            ["CANO"]          = anio,
        };

        try
        {
            await _sdk.ActualizarDocumentoPorIdAsync(idDocumento, campos);
        }
        catch (Exception ex)
        {
            // Estos campos son CRÍTICOS para el CFDI 4.0 (InformacionGlobal).
            throw new Exception(
                $"Error al configurar campos de Información Global (Periodicidad, Mes, Año): {ex.Message}. " +
                "Verifique que el concepto esté configurado como Factura Global y soporte CFDI 4.0.");
        }

        // 5. Timbrar desatendido
        bool timbrado = false;
        TimbradoResult? datosFiscales = null;

        if (request.AutoTimbrar)
        {
            var csdPassword = request.CsdPassword;
            if (string.IsNullOrWhiteSpace(csdPassword))
            {
                csdPassword = _configuration["Contpaqi:CsdPassword"];
            }

            if (string.IsNullOrWhiteSpace(csdPassword))
                throw new ArgumentException("La contraseña del CSD es requerida para timbrar la factura global automáticamente.");

            if (double.TryParse(folioReal, out double folioNum))
            {
                var camposALeer = new[] { "CUUID", "CCADENAORIGINAL", "CSELLOEMISOR", "CSATSELLO", "CCERTIFICADOEMISOR", "CCERTIFICADOSAT", "CFECHA", "CHORA" };
                var datosLeidos = await _sdk.EmitirDocumentoYLeerDatosAsync(
                    request.CodigoConcepto,
                    request.Serie,
                    folioNum,
                    csdPassword,
                    string.Empty,
                    camposALeer);

                timbrado = true;

                datosFiscales = new TimbradoResult
                {
                    UUID               = datosLeidos.GetValueOrDefault("CUUID", string.Empty),
                    CadenaOriginal     = datosLeidos.GetValueOrDefault("CCADENAORIGINAL", string.Empty),
                    SelloDigitalEmisor = datosLeidos.GetValueOrDefault("CSELLOEMISOR", string.Empty),
                    SelloDigitalSAT    = datosLeidos.GetValueOrDefault("CSATSELLO", string.Empty),
                    NoCertificadoEmisor= datosLeidos.GetValueOrDefault("CCERTIFICADOEMISOR", string.Empty),
                    NoCertificadoSAT   = datosLeidos.GetValueOrDefault("CCERTIFICADOSAT", string.Empty)
                };

                var fecha = datosLeidos.GetValueOrDefault("CFECHA", string.Empty);
                var hora  = datosLeidos.GetValueOrDefault("CHORA", string.Empty);
                if (!string.IsNullOrWhiteSpace(fecha) || !string.IsNullOrWhiteSpace(hora))
                {
                    datosFiscales.FechaTimbrado = $"{fecha} {hora}".Trim();
                }
            }
        }

        return new CreateFacturaResult
        {
            IdDocumento   = idDocumento,
            Serie         = serieReal?.Trim() ?? string.Empty,
            Folio         = folioReal?.Trim() ?? string.Empty,
            Timbrado      = timbrado,
            DatosFiscales = datosFiscales,
            Mensaje       = timbrado ? "Factura Global timbrada exitosamente." : "Factura Global creada sin timbrar."
        };
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    /// <summary>Trunca una referencia a los primeros 30 caracteres (límite del SDK).</summary>
    private static string TruncateRef(string s)
        => s.Length > 30 ? s[..30] : s;
}
