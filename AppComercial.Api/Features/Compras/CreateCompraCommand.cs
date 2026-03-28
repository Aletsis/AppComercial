using MediatR;
using AppComercial.Api.Sdk;
using System.ComponentModel.DataAnnotations;

namespace AppComercial.Api.Features.Compras;

/// <summary>
/// Crea un documento de Compra completo (cabecera + partidas) en CONTPAQi Comercial.
/// El código de concepto debe ser uno configurado con naturaleza Compra (CNATURALEZA = 2).
/// </summary>
public class CreateCompraCommand : IRequest<int>
{
    /// <summary>
    /// Código del Concepto de tipo Compra configurado en CONTPAQi. Requerido.
    /// Ejemplo: "COMP"
    /// </summary>
    [Required(ErrorMessage = "El código del concepto es requerido.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código del concepto debe tener entre 1 y 30 caracteres.")]
    public string CodigoConcepto { get; set; } = string.Empty;

    /// <summary>
    /// Serie del documento. Puede ser vacío si el concepto no maneja series.
    /// Ejemplo: "A"
    /// </summary>
    [StringLength(10, ErrorMessage = "La serie no puede exceder 10 caracteres.")]
    public string Serie { get; set; } = string.Empty;

    /// <summary>
    /// Folio del documento. Opcional. Si se envía 0, CONTPAQi asignará el siguiente folio automático consecutivo.
    /// </summary>
    public double Folio { get; set; } = 0;

    /// <summary>
    /// Código del proveedor al que se realiza la compra. Requerido.
    /// Ejemplo: "PROV001"
    /// </summary>
    [Required(ErrorMessage = "El código del proveedor es requerido.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código del proveedor debe tener entre 1 y 30 caracteres.")]
    public string CodigoProveedor { get; set; } = string.Empty;

    /// <summary>
    /// Referencia o número de factura del proveedor. Opcional.
    /// Ejemplo: "FAC-20240315"
    /// </summary>
    [StringLength(30, ErrorMessage = "La referencia no puede exceder 30 caracteres.")]
    public string Referencia { get; set; } = string.Empty;

    /// <summary>
    /// Observaciones o comentarios sobre la compra. Opcional.
    /// </summary>
    public string Observaciones { get; set; } = string.Empty;

    /// <summary>
    /// Código del almacén destino donde entran las mercancías compradas. Requerido.
    /// Ejemplo: "ALM01"
    /// </summary>
    [Required(ErrorMessage = "El código del almacén es requerido.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código del almacén debe tener entre 1 y 30 caracteres.")]
    public string CodigoAlmacen { get; set; } = string.Empty;

    /// <summary>
    /// Número de moneda a utilizar. Por defecto: 1 (Peso Mexicano).
    /// Consultar el catálogo de monedas para otros valores.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "El número de moneda debe ser mayor a 0.")]
    public int NumeroMoneda { get; set; } = 1;

    /// <summary>
    /// Tipo de cambio respecto al peso mexicano. Por defecto: 1.0 (misma moneda).
    /// </summary>
    [Range(0.0001, double.MaxValue, ErrorMessage = "El tipo de cambio debe ser mayor a 0.")]
    public double TipoCambio { get; set; } = 1.0;

    /// <summary>
    /// Lista de partidas (productos) incluidos en la compra. Se requiere al menos una partida.
    /// </summary>
    [MinLength(1, ErrorMessage = "La compra debe tener al menos una partida.")]
    public List<CompraPartida> Partidas { get; set; } = new();
}

/// <summary>
/// Representa una línea de producto dentro de un documento de Compra.
/// </summary>
public class CompraPartida
{
    /// <summary>
    /// Código del producto o servicio comprado. Requerido.
    /// Ejemplo: "PROD-001"
    /// </summary>
    [Required(ErrorMessage = "El código del producto es requerido en cada partida.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código del producto debe tener entre 1 y 30 caracteres.")]
    public string CodigoProducto { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad de unidades compradas. Debe ser mayor a 0.
    /// </summary>
    [Range(0.0001, double.MaxValue, ErrorMessage = "Las unidades deben ser mayores a 0.")]
    public double Unidades { get; set; }

    /// <summary>
    /// Precio o costo unitario de compra (sin impuestos). Debe ser mayor o igual a 0.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor o igual a 0.")]
    public double PrecioUnitario { get; set; }
}

public class CreateCompraCommandHandler : IRequestHandler<CreateCompraCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public CreateCompraCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(CreateCompraCommand request, CancellationToken cancellationToken)
    {
        // 1. Crear la cabecera del documento de compra
        var documento = new tDocumento
        {
            aCodConcepto   = request.CodigoConcepto,
            aSerie         = request.Serie,
            aFolio         = request.Folio,
            aCodigoCteProv = request.CodigoProveedor,
            aReferencia    = request.Referencia,
            aFecha         = DateTime.Now.ToString("MM/dd/yyyy"),
            aNumMoneda     = request.NumeroMoneda,
            aTipoCambio    = request.TipoCambio
        };

        var idDocumento = await _sdk.CrearDocumentoAsync(documento);

        // Si hay observaciones, actualizar el documento para inyectarlas
        if (!string.IsNullOrWhiteSpace(request.Observaciones))
        {
            var datosEdicion = new Dictionary<string, string>
            {
                { "COBSERVACIONES", request.Observaciones }
            };
            await _sdk.ActualizarDocumentoPorIdAsync(idDocumento, datosEdicion);
        }

        // 2. Agregar las partidas de compra (movimientos)
        foreach (var partida in request.Partidas)
        {
            var movimiento = new tMovimiento
            {
                aConsecutivo      = 0,
                aCodProdSer       = partida.CodigoProducto,
                aUnidades         = partida.Unidades,
                aPrecio           = partida.PrecioUnitario,
                aCosto            = partida.PrecioUnitario,
                aCodAlmacen       = request.CodigoAlmacen,
                aReferencia       = string.Empty,
                aCodClasificacion = string.Empty
            };
            await _sdk.CrearMovimientoAsync(idDocumento, movimiento);
        }

        return idDocumento;
    }
}
