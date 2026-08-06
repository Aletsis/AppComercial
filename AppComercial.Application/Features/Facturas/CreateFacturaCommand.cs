using MediatR;
using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Interfaces.SdkModels;
using System.ComponentModel.DataAnnotations;
using AppComercial.Application.DTOs;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using AppComercial.Application.Common.Interfaces;

namespace AppComercial.Application.Features.Facturas;

/// <summary>
/// Comando para crear una Factura de Venta completa (cabecera + partidas) en CONTPAQi Comercial.
/// CFDI 4.0: incluye UsoCFDI, MetodoPago, FormaPago y timbrado desatendido.
/// </summary>
public class CreateFacturaCommand : IRequest<CreateFacturaResult>
{
    [Required] public string CodigoConcepto { get; set; } = string.Empty;
    [StringLength(10)] public string Serie { get; set; } = string.Empty;
    [Required] public string CodigoCliente { get; set; } = string.Empty;
    [StringLength(30)] public string Referencia { get; set; } = string.Empty;
    [StringLength(30)] public string CodigoAgente { get; set; } = string.Empty;
    public int NumeroMoneda { get; set; } = 1;
    public double TipoCambio { get; set; } = 1.0;

    // ─── CFDI 4.0 ──────────────────────────────────────────────────────────
    /// <summary>Uso del CFDI (catálogo SAT). Ej: G01, G03, D01, S01.</summary>
    public string UsoCfdi { get; set; } = "G03"; // Gastos en general

    /// <summary>Método de pago. PUE = Pago en una sola exhibición. PPD = Parcialidades.</summary>
    public string MetodoPago { get; set; } = "PUE";

    /// <summary>Forma de pago según catálogo SAT: 01=Efectivo, 04=TDC, 28=Débito, 03=Transferencia.</summary>
    public string FormaPago { get; set; } = "01";

    /// <summary>Contraseña del Sello Digital (CSD). Se usa para el timbrado automático.</summary>
    public string CsdPassword { get; set; } = string.Empty;

    /// <summary>Email del receptor al que se enviará la factura. Opcional.</summary>
    public string CsdEmail { get; set; } = string.Empty;

    /// <summary>Si es true, emite (timbra) el documento automáticamente. Default: true.</summary>
    public bool AutoTimbrar { get; set; } = true;

    [MinLength(1, ErrorMessage = "La factura debe tener al menos una partida.")]
    public List<FacturaPartida> Partidas { get; set; } = new();
}

public class FacturaPartida
{
    [Required] public string CodigoProducto { get; set; } = string.Empty;
    [Range(0.0001, double.MaxValue)] public double Unidades { get; set; }
    [Range(0, double.MaxValue)] public double PrecioUnitario { get; set; }
    [Required] public string CodigoAlmacen { get; set; } = string.Empty;
}

public class CreateFacturaResult
{
    public int IdDocumento { get; set; }
    public string Serie { get; set; } = string.Empty;
    public string Folio { get; set; } = string.Empty;
    public bool Timbrado { get; set; }
    public string? Mensaje { get; set; }
    public TimbradoResult? DatosFiscales { get; set; }
}

public class CreateFacturaCommandHandler : IRequestHandler<CreateFacturaCommand, CreateFacturaResult>
{
    private readonly IContpaqiSdk _sdk;
    private readonly IConfiguration _configuration;
    private readonly ICurrentUserService _currentUserService;

    public CreateFacturaCommandHandler(IContpaqiSdk sdk, IConfiguration configuration, ICurrentUserService currentUserService)
    {
        _sdk = sdk;
        _configuration = configuration;
        _currentUserService = currentUserService;
    }

    public async Task<CreateFacturaResult> Handle(CreateFacturaCommand request, CancellationToken cancellationToken)
    {
        // 1. Crear el documento cabecera
        var documento = new tDocumento
        {
            aCodConcepto   = request.CodigoConcepto,
            aSerie         = request.Serie,
            aCodigoCteProv = request.CodigoCliente,
            aReferencia    = request.Referencia,
            aFecha         = DateTime.Now.ToString("MM/dd/yyyy"),
            aNumMoneda     = request.NumeroMoneda,
            aTipoCambio    = request.TipoCambio
        };

        var idDocumento = await _sdk.CrearDocumentoAsync(documento);

        // 2. Setear campos CFDI 4.0 vía fSetDatoDocumento (ANTES de las partidas)
        // Según comunidad (Andres Ramos SDK): FormaPago -> CMETODOPAG, MetodoPago -> CCANTPARCI (1=PUE, 2=PPD)
        var camposCfdi = new Dictionary<string, string>
        {
            ["cUsoCFDI"]    = request.UsoCfdi,
            ["CMETODOPAG"]  = request.FormaPago,
            ["CCANTPARCI"]  = request.MetodoPago.ToUpper() == "PPD" ? "2" : "1",
            ["CTEXTOEXTRA2"] = _currentUserService.GetCurrentUsuario() ?? "SISTEMA"
        };
        if (!string.IsNullOrEmpty(request.CodigoAgente))
            camposCfdi["CCODIGOAGENTE"] = request.CodigoAgente;

        string? errorFiscal = null;
        try 
        { 
            await _sdk.ActualizarDocumentoPorIdAsync(idDocumento, camposCfdi); 
        } 
        catch (Exception ex)
        { 
            errorFiscal = $"Error campos fiscales: {ex.Message}";
        }

        // 3. Agregar partidas
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

        // 4. Leer Folio y Serie reales asignados por el SDK (con Reintento y Fallback)
        string folioReal = "0";
        string serieReal = request.Serie;
        try 
        { 
            folioReal = await _sdk.LeerDatoDocumentoAsync("CFOLIO"); 
        } 
        catch 
        { 
            try { folioReal = await _sdk.LeerDatoDocumentoAsync("cFolio"); } catch { folioReal = idDocumento.ToString(); }
        }

        try 
        { 
            serieReal = await _sdk.LeerDatoDocumentoAsync("CSERIEDOCUMENTO"); 
        } 
        catch 
        { 
            try 
            { 
                serieReal = await _sdk.LeerDatoDocumentoAsync("CSERIE"); 
            } 
            catch 
            { 
                try { serieReal = await _sdk.LeerDatoDocumentoAsync("cSerie"); } catch { /* mantener serie del request */ }
            }
        }

        // 6. Timbrar (emitir) el documento con el CSD provisto en la petición o configuración
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
                throw new ArgumentException("La contraseña del CSD es requerida para timbrar automáticamente.");

            // Para el timbrado necesitamos el Folio (double)
            if (double.TryParse(folioReal, out double folioNum))
            {
                var camposALeer = new[] { "CUUID", "CCADENAORIGINAL", "CSELLOEMISOR", "CSATSELLO", "CCERTIFICADOEMISOR", "CCERTIFICADOSAT", "CFECHA", "CHORA" };
                var datosLeidos = await _sdk.EmitirDocumentoYLeerDatosAsync(
                    request.CodigoConcepto,
                    request.Serie,
                    folioNum,
                    csdPassword,
                    request.CsdEmail,
                    camposALeer);
                
                timbrado = true;

                // Recuperar datos fiscales tras el timbrado de forma segura
                datosFiscales = new TimbradoResult
                {
                    UUID = datosLeidos.GetValueOrDefault("CUUID", string.Empty),
                    CadenaOriginal = datosLeidos.GetValueOrDefault("CCADENAORIGINAL", string.Empty),
                    SelloDigitalEmisor = datosLeidos.GetValueOrDefault("CSELLOEMISOR", string.Empty),
                    SelloDigitalSAT = datosLeidos.GetValueOrDefault("CSATSELLO", string.Empty),
                    NoCertificadoEmisor = datosLeidos.GetValueOrDefault("CCERTIFICADOEMISOR", string.Empty),
                    NoCertificadoSAT = datosLeidos.GetValueOrDefault("CCERTIFICADOSAT", string.Empty)
                };

                var fecha = datosLeidos.GetValueOrDefault("CFECHA", string.Empty);
                var hora = datosLeidos.GetValueOrDefault("CHORA", string.Empty);
                if (!string.IsNullOrWhiteSpace(fecha) || !string.IsNullOrWhiteSpace(hora))
                {
                    datosFiscales.FechaTimbrado = $"{fecha} {hora}".Trim();
                }
            }
        }

        var mensajeFinal = timbrado
            ? "Factura timbrada exitosamente."
            : "Factura creada. No se timbró automáticamente.";
        
        if (!string.IsNullOrEmpty(errorFiscal))
            mensajeFinal += $" (Aviso: {errorFiscal})";

        return new CreateFacturaResult
        {
            IdDocumento = idDocumento,
            Serie       = serieReal?.Trim() ?? string.Empty,
            Folio       = folioReal?.Trim() ?? string.Empty,
            Timbrado    = timbrado,
            DatosFiscales = datosFiscales,
            Mensaje     = mensajeFinal
        };
    }
}
