using MediatR;
using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Interfaces.SdkModels;
using System.ComponentModel.DataAnnotations;

namespace AppComercial.Application.Features.NotasCredito;

/// <summary>
/// Crea una Nota de Crédito (Egreso) CFDI 4.0 relacionada a una Factura de origen.
/// Flujo: Crea documento → Agrega partidas → Registra relación CFDI (UUID) → Timbra → Salda.
/// </summary>
public class CreateNotaCreditoCommand : IRequest<CreateNotaCreditoResult>
{
    [Required] public string CodigoConcepto { get; set; } = string.Empty;
    [StringLength(10)] public string Serie { get; set; } = string.Empty;
    [Required] public string CodigoCliente { get; set; } = string.Empty;
    [StringLength(30)] public string ReferenciaDocumentoOrigen { get; set; } = string.Empty;
    public int NumeroMoneda { get; set; } = 1;
    public double TipoCambio { get; set; } = 1.0;

    // ─── CFDI 4.0 ──────────────────────────────────────────────────────────
    public string UsoCfdi { get; set; } = "G02";   // Devoluciones, descuentos o bonificaciones
    public string MetodoPago { get; set; } = "PUE";
    public string FormaPago { get; set; } = "15";   // Condonación - ajustar según el caso

    /// <summary>UUID de la factura de venta que se afecta con esta nota de crédito.</summary>
    [Required(ErrorMessage = "El UUID de la factura origen es requerido para la relación CFDI.")]
    public string UuidFacturaOrigen { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de relación SAT para el CfdiRelacionados:
    /// 01 = Nota de crédito de documentos relacionados.
    /// 03 = Devolución de mercancía sobre facturas o traslados previos.
    /// </summary>
    public string TipoRelacionSat { get; set; } = "01";

    // ─── Para el Saldado Cruzado (CXC) ─────────────────────────────────────
    public string ConceptoFacturaOrigen { get; set; } = string.Empty;
    public string SerieFacturaOrigen { get; set; } = string.Empty;
    public double FolioFacturaOrigen { get; set; }
    public bool SaldarFacturaOrigen { get; set; } = true;

    /// <summary>Contraseña CSD para timbrado desatendido.</summary>
    [Required] public string CsdPassword { get; set; } = string.Empty;
    public bool AutoTimbrar { get; set; } = true;

    [MinLength(1)] public List<NotaCreditoPartida> Partidas { get; set; } = new();
}

public class NotaCreditoPartida
{
    [Required] public string CodigoProducto { get; set; } = string.Empty;
    [Range(0.0001, double.MaxValue)] public double Unidades { get; set; }
    [Range(0, double.MaxValue)] public double PrecioUnitario { get; set; }
    [Required] public string CodigoAlmacen { get; set; } = string.Empty;
}

public class CreateNotaCreditoResult
{
    public int IdDocumento { get; set; }
    public bool Timbrado { get; set; }
    public bool Saldado { get; set; }
    public string? Mensaje { get; set; }
}

public class CreateNotaCreditoCommandHandler : IRequestHandler<CreateNotaCreditoCommand, CreateNotaCreditoResult>
{
    private readonly IContpaqiSdk _sdk;

    public CreateNotaCreditoCommandHandler(IContpaqiSdk sdk) => _sdk = sdk;

    public async Task<CreateNotaCreditoResult> Handle(CreateNotaCreditoCommand request, CancellationToken cancellationToken)
    {
        // 1. Crear el documento Nota de Crédito (Egreso)
        var documento = new tDocumento
        {
            aCodConcepto   = request.CodigoConcepto,
            aSerie         = request.Serie,
            aCodigoCteProv = request.CodigoCliente,
            aReferencia    = request.ReferenciaDocumentoOrigen,
            aFecha         = DateTime.Now.ToString("MM/dd/yyyy"),
            aNumMoneda     = request.NumeroMoneda,
            aTipoCambio    = request.TipoCambio
        };

        var idDocumento = await _sdk.CrearDocumentoAsync(documento);

        // 2. Setear campos CFDI 4.0
        var campos = new Dictionary<string, string>
        {
            ["USOCFDI"]    = request.UsoCfdi,
            ["METODOPAGO"] = request.MetodoPago,
            ["FORMAPAGO"]  = request.FormaPago,
        };
        await _sdk.ActualizarDocumentoPorIdAsync(idDocumento, campos);

        // 3. Agregar partidas de devolución
        foreach (var partida in request.Partidas)
        {
            var movimiento = new tMovimiento
            {
                aConsecutivo      = 0,
                aCodProdSer       = partida.CodigoProducto,
                aUnidades         = partida.Unidades,
                aPrecio           = partida.PrecioUnitario,
                aCosto            = 0,
                aCodAlmacen       = partida.CodigoAlmacen,
                aReferencia       = string.Empty,
                aCodClasificacion = string.Empty
            };
            await _sdk.CrearMovimientoAsync(idDocumento, movimiento);
        }

        // 4. Registrar relación CFDI con la factura origen
        // Esto genera el nodo <cfdi:CfdiRelacionados TipoRelacion="01"> en el XML del timbrado.
        if (!string.IsNullOrWhiteSpace(request.UuidFacturaOrigen))
        {
            await _sdk.AgregarRelacionCfdiAsync(request.UuidFacturaOrigen, request.TipoRelacionSat);
        }

        // 5. Timbrar (emitir) la nota de crédito
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

        // 6. Saldar la factura origen en CXC para eliminar el adeudo del cliente
        bool saldado = false;
        if (request.SaldarFacturaOrigen
            && !string.IsNullOrWhiteSpace(request.ConceptoFacturaOrigen)
            && request.FolioFacturaOrigen > 0)
        {
            await _sdk.SaldarDocumentoAsync(
                request.ConceptoFacturaOrigen,
                request.SerieFacturaOrigen,
                request.FolioFacturaOrigen,
                request.CodigoConcepto,
                request.Serie,
                idDocumento);
            saldado = true;
        }

        return new CreateNotaCreditoResult
        {
            IdDocumento = idDocumento,
            Timbrado    = timbrado,
            Saldado     = saldado,
            Mensaje     = $"Nota de Crédito creada (id: {idDocumento}). Timbrada: {timbrado}. CXC Saldada: {saldado}."
        };
    }
}
