using MediatR;
using AppComercial.Api.Sdk;
using System.ComponentModel.DataAnnotations;

namespace AppComercial.Api.Features.Cotizaciones;

/// <summary>
/// Comando para crear una Cotización completa (cabecera + partidas) en CONTPAQi Comercial.
/// El código de concepto debe ser uno configurado con naturaleza Venta (CNATURALEZA = 1)
/// y de tipo cotización. Las cotizaciones NO afectan inventario.
/// </summary>
public class CreateCotizacionCommand : IRequest<int>
{
    /// <summary>
    /// Código del Concepto de tipo Cotización configurado en CONTPAQi. Requerido.
    /// Ejemplo: "COT", "COTI"
    /// </summary>
    [Required(ErrorMessage = "El código del concepto es requerido.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código del concepto debe tener entre 1 y 30 caracteres.")]
    public string CodigoConcepto { get; set; } = string.Empty;

    /// <summary>
    /// Serie de la cotización. Opcional. Máximo 10 caracteres.
    /// </summary>
    [StringLength(10, ErrorMessage = "La serie no puede exceder 10 caracteres.")]
    public string Serie { get; set; } = string.Empty;

    /// <summary>
    /// Código del cliente al que se dirige la cotización. Requerido. Máximo 30 caracteres.
    /// Ejemplo: "CLI001"
    /// </summary>
    [Required(ErrorMessage = "El código del cliente es requerido.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código del cliente debe tener entre 1 y 30 caracteres.")]
    public string CodigoCliente { get; set; } = string.Empty;

    /// <summary>
    /// Referencia o número de solicitud del cliente. Opcional. Máximo 30 caracteres.
    /// </summary>
    [StringLength(30, ErrorMessage = "La referencia no puede exceder 30 caracteres.")]
    public string Referencia { get; set; } = string.Empty;

    /// <summary>
    /// Código del agente de venta que elabora la cotización. Opcional. Máximo 30 caracteres.
    /// </summary>
    [StringLength(30, ErrorMessage = "El código del agente no puede exceder 30 caracteres.")]
    public string CodigoAgente { get; set; } = string.Empty;

    /// <summary>
    /// Número de moneda. Por defecto: 1 (Peso Mexicano).
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "El número de moneda debe ser mayor a 0.")]
    public int NumeroMoneda { get; set; } = 1;

    /// <summary>
    /// Tipo de cambio. Por defecto: 1.0.
    /// </summary>
    [Range(0.0001, double.MaxValue, ErrorMessage = "El tipo de cambio debe ser mayor a 0.")]
    public double TipoCambio { get; set; } = 1.0;

    /// <summary>
    /// Lista de productos/servicios cotizados. Se requiere al menos uno.
    /// </summary>
    [MinLength(1, ErrorMessage = "La cotización debe tener al menos una partida.")]
    public List<CotizacionPartida> Partidas { get; set; } = new();
}

/// <summary>
/// Representa una línea de producto o servicio dentro de una Cotización.
/// </summary>
public class CotizacionPartida
{
    /// <summary>
    /// Código del producto o servicio cotizado. Requerido. Máximo 30 caracteres.
    /// </summary>
    [Required(ErrorMessage = "El código del producto es requerido en cada partida.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código del producto debe tener entre 1 y 30 caracteres.")]
    public string CodigoProducto { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad de unidades cotizadas. Debe ser mayor a 0.
    /// </summary>
    [Range(0.0001, double.MaxValue, ErrorMessage = "Las unidades deben ser mayores a 0.")]
    public double Unidades { get; set; }

    /// <summary>
    /// Precio unitario cotizado (sin impuestos). Debe ser mayor o igual a 0.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor o igual a 0.")]
    public double PrecioUnitario { get; set; }
}

public class CreateCotizacionCommandHandler : IRequestHandler<CreateCotizacionCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public CreateCotizacionCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(CreateCotizacionCommand request, CancellationToken cancellationToken)
    {
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

        foreach (var partida in request.Partidas)
        {
            var movimiento = new tMovimiento
            {
                aConsecutivo      = 0,
                aCodProdSer       = partida.CodigoProducto,
                aUnidades         = partida.Unidades,
                aPrecio           = partida.PrecioUnitario,
                aCosto            = 0,
                aCodAlmacen       = string.Empty, // Las cotizaciones no afectan inventario
                aReferencia       = string.Empty,
                aCodClasificacion = string.Empty
            };
            await _sdk.CrearMovimientoAsync(idDocumento, movimiento);
        }

        return idDocumento;
    }
}
