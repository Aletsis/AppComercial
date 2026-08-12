using MediatR;
using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Interfaces.SdkModels;
using System.ComponentModel.DataAnnotations;
using AppComercial.Application.Common.Interfaces;
using System.Collections.Generic;

namespace AppComercial.Application.Features.Traspasos;

/// <summary>
/// Crea un documento de Traspaso entre almacenes completo (cabecera + partidas) en CONTPAQi Comercial.
/// </summary>
public class CreateTraspasoCommand : IRequest<CreateTraspasoResult>
{
    /// <summary>
    /// Código del Concepto de tipo Traspaso configurado en CONTPAQi. Requerido.
    /// Ejemplo: "TRAS", "TR-ABARROTES"
    /// </summary>
    [Required(ErrorMessage = "El código del concepto es requerido.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código del concepto debe tener entre 1 y 30 caracteres.")]
    public string CodigoConcepto { get; set; } = string.Empty;

    /// <summary>
    /// Serie del documento. Opcional.
    /// </summary>
    [StringLength(10, ErrorMessage = "La serie no puede exceder 10 caracteres.")]
    public string Serie { get; set; } = string.Empty;

    /// <summary>
    /// Folio del documento. Si es 0, CONTPAQi asignará el siguiente folio automático.
    /// </summary>
    public double Folio { get; set; } = 0;

    /// <summary>
    /// Código del almacén de origen (de donde sale la mercancía). Requerido.
    /// </summary>
    [Required(ErrorMessage = "El código del almacén de origen es requerido.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código del almacén origen debe tener entre 1 y 30 caracteres.")]
    public string CodigoAlmacenOrigen { get; set; } = string.Empty;

    /// <summary>
    /// Código del almacén de destino (a donde entra la mercancía). Requerido.
    /// </summary>
    [Required(ErrorMessage = "El código del almacén de destino es requerido.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código del almacén destino debe tener entre 1 y 30 caracteres.")]
    public string CodigoAlmacenDestino { get; set; } = string.Empty;

    /// <summary>
    /// Referencia o motivo del traspaso. Opcional.
    /// </summary>
    [StringLength(30, ErrorMessage = "La referencia no puede exceder 30 caracteres.")]
    public string Referencia { get; set; } = string.Empty;

    /// <summary>
    /// Observaciones o comentarios sobre el traspaso. Opcional.
    /// </summary>
    public string Observaciones { get; set; } = string.Empty;

    /// <summary>
    /// Lista de productos a traspasar. Se requiere al menos una partida.
    /// </summary>
    [MinLength(1, ErrorMessage = "El traspaso debe tener al menos una partida.")]
    public List<TraspasoPartida> Partidas { get; set; } = new();
}

public class TraspasoPartida
{
    /// <summary>
    /// Código del producto. Requerido.
    /// </summary>
    [Required(ErrorMessage = "El código del producto es requerido en cada partida.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código del producto debe tener entre 1 y 30 caracteres.")]
    public string CodigoProducto { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad de unidades a traspasar. Debe ser mayor a 0.
    /// </summary>
    [Range(0.0001, double.MaxValue, ErrorMessage = "Las unidades deben ser mayores a 0.")]
    public double Unidades { get; set; }
}

public class CreateTraspasoResult
{
    public int IdDocumento { get; set; }
    public string CodigoConcepto { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public string Folio { get; set; } = string.Empty;
}

public class CreateTraspasoCommandHandler : IRequestHandler<CreateTraspasoCommand, CreateTraspasoResult>
{
    private readonly IContpaqiSdk _sdk;
    private readonly ICurrentUserService _currentUserService;

    public CreateTraspasoCommandHandler(IContpaqiSdk sdk, ICurrentUserService currentUserService)
    {
        _sdk = sdk;
        _currentUserService = currentUserService;
    }

    public async Task<CreateTraspasoResult> Handle(CreateTraspasoCommand request, CancellationToken cancellationToken)
    {
        // 1. Crear la cabecera del documento de traspaso
        var documento = new tDocumento
        {
            aCodConcepto   = request.CodigoConcepto,
            aSerie         = request.Serie,
            aFolio         = request.Folio,
            aReferencia    = request.Referencia,
            aFecha         = DateTime.Now.ToString("MM/dd/yyyy"),
            aCodigoCteProv = string.Empty,
            aNumMoneda     = 1,
            aTipoCambio    = 1.0
        };

        var idDocumento = await _sdk.CrearDocumentoAsync(documento);

        var datosEdicion = new Dictionary<string, string>
        {
            ["CTEXTOEXTRA2"] = _currentUserService.GetCurrentUsuario() ?? "SISTEMA"
        };
        if (!string.IsNullOrWhiteSpace(request.Observaciones))
        {
            datosEdicion["COBSERVACIONES"] = request.Observaciones;
        }
        await _sdk.ActualizarDocumentoPorIdAsync(idDocumento, datosEdicion);

        // 2. Agregar cada partida (movimiento)
        foreach (var partida in request.Partidas)
        {
            var movimiento = new tMovimiento
            {
                aConsecutivo      = 0,
                aCodProdSer       = partida.CodigoProducto,
                aUnidades         = partida.Unidades,
                aCosto            = 0,
                aPrecio           = 0,
                aCodAlmacen       = request.CodigoAlmacenOrigen,
                aReferencia       = request.CodigoAlmacenDestino,
                aCodClasificacion = string.Empty
            };
            var idMovimiento = await _sdk.CrearMovimientoAsync(idDocumento, movimiento);

            // Si se requiere registrar almacén destino explícito en campos del movimiento
            if (!string.IsNullOrWhiteSpace(request.CodigoAlmacenDestino))
            {
                try
                {
                    var datosMov = new Dictionary<string, string>
                    {
                        ["CTEXTOEXTRA1"] = request.CodigoAlmacenDestino
                    };
                    await _sdk.ActualizarMovimientoAsync(idDocumento, idMovimiento, datosMov);
                }
                catch
                {
                    // Fallback si no aplica
                }
            }
        }

        // 3. Leer Folio y Serie reales asignados por el SDK
        string folioReal = request.Folio > 0 ? request.Folio.ToString() : "0";
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

        return new CreateTraspasoResult
        {
            IdDocumento = idDocumento,
            CodigoConcepto = request.CodigoConcepto,
            Serie = serieReal?.Trim() ?? string.Empty,
            Folio = folioReal?.Trim() ?? string.Empty
        };
    }
}
