using MediatR;
using AppComercial.Api.Sdk;
using System.ComponentModel.DataAnnotations;

namespace AppComercial.Api.Features.Facturas;

/// <summary>
/// Comando para crear una Factura de Venta completa (cabecera + partidas) en CONTPAQi Comercial.
/// El código de concepto debe ser uno configurado con naturaleza Venta (CNATURALEZA = 1).
/// </summary>
public class CreateFacturaCommand : IRequest<int>
{
    /// <summary>
    /// Código del Concepto de tipo Factura configurado en CONTPAQi. Requerido.
    /// Ejemplo: "FAC", "FACT"
    /// </summary>
    [Required(ErrorMessage = "El código del concepto es requerido.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código del concepto debe tener entre 1 y 30 caracteres.")]
    public string CodigoConcepto { get; set; } = string.Empty;

    /// <summary>
    /// Serie de la factura. Opcional. Máximo 10 caracteres.
    /// Ejemplo: "A"
    /// </summary>
    [StringLength(10, ErrorMessage = "La serie no puede exceder 10 caracteres.")]
    public string Serie { get; set; } = string.Empty;

    /// <summary>
    /// Código del cliente al que se emite la factura. Requerido. Máximo 30 caracteres.
    /// Ejemplo: "CLI001"
    /// </summary>
    [Required(ErrorMessage = "El código del cliente es requerido.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código del cliente debe tener entre 1 y 30 caracteres.")]
    public string CodigoCliente { get; set; } = string.Empty;

    /// <summary>
    /// Referencia de la factura (ej. número de orden de compra del cliente). Opcional.
    /// Máximo 30 caracteres.
    /// </summary>
    [StringLength(30, ErrorMessage = "La referencia no puede exceder 30 caracteres.")]
    public string Referencia { get; set; } = string.Empty;

    /// <summary>
    /// Código del agente de venta responsable. Opcional. Máximo 30 caracteres.
    /// Ejemplo: "AGT001"
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
    /// Lista de partidas (productos/servicios) de la factura. Se requiere al menos una.
    /// </summary>
    [MinLength(1, ErrorMessage = "La factura debe tener al menos una partida.")]
    public List<FacturaPartida> Partidas { get; set; } = new();
}

/// <summary>
/// Representa una línea de producto o servicio dentro de una Factura.
/// </summary>
public class FacturaPartida
{
    /// <summary>
    /// Código del producto o servicio. Requerido. Máximo 30 caracteres.
    /// Ejemplo: "PROD-001"
    /// </summary>
    [Required(ErrorMessage = "El código del producto es requerido en cada partida.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código del producto debe tener entre 1 y 30 caracteres.")]
    public string CodigoProducto { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad de unidades. Debe ser mayor a 0.
    /// </summary>
    [Range(0.0001, double.MaxValue, ErrorMessage = "Las unidades deben ser mayores a 0.")]
    public double Unidades { get; set; }

    /// <summary>
    /// Precio unitario de venta (sin impuestos). Debe ser mayor o igual a 0.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor o igual a 0.")]
    public double PrecioUnitario { get; set; }

    /// <summary>
    /// Código del almacén del que se descuenta la mercancía. Requerido. Máximo 30 caracteres.
    /// Ejemplo: "ALM01"
    /// </summary>
    [Required(ErrorMessage = "El código del almacén es requerido en cada partida.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código del almacén debe tener entre 1 y 30 caracteres.")]
    public string CodigoAlmacen { get; set; } = string.Empty;
}

public class CreateFacturaCommandHandler : IRequestHandler<CreateFacturaCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public CreateFacturaCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(CreateFacturaCommand request, CancellationToken cancellationToken)
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
                aCodAlmacen       = partida.CodigoAlmacen,
                aReferencia       = string.Empty,
                aCodClasificacion = string.Empty
            };
            await _sdk.CrearMovimientoAsync(idDocumento, movimiento);
        }

        return idDocumento;
    }
}
