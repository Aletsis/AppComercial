using MediatR;
using AppComercial.Api.Sdk;
using System.ComponentModel.DataAnnotations;

namespace AppComercial.Api.Features.NotasCredito;

/// <summary>
/// Comando para crear una Nota de Crédito / Devolución sobre Venta en CONTPAQi Comercial.
/// El código de concepto debe ser uno configurado con naturaleza Devolución sobre Venta (CNATURALEZA = 3).
/// </summary>
public class CreateNotaCreditoCommand : IRequest<int>
{
    /// <summary>
    /// Código del Concepto de tipo Devolución/Nota de Crédito configurado en CONTPAQi. Requerido.
    /// Ejemplo: "DEV", "NC", "DEVVTA"
    /// </summary>
    [Required(ErrorMessage = "El código del concepto es requerido.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código del concepto debe tener entre 1 y 30 caracteres.")]
    public string CodigoConcepto { get; set; } = string.Empty;

    /// <summary>
    /// Serie de la nota de crédito. Opcional. Máximo 10 caracteres.
    /// </summary>
    [StringLength(10, ErrorMessage = "La serie no puede exceder 10 caracteres.")]
    public string Serie { get; set; } = string.Empty;

    /// <summary>
    /// Código del cliente que realiza la devolución. Requerido. Máximo 30 caracteres.
    /// Ejemplo: "CLI001"
    /// </summary>
    [Required(ErrorMessage = "El código del cliente es requerido.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código del cliente debe tener entre 1 y 30 caracteres.")]
    public string CodigoCliente { get; set; } = string.Empty;

    /// <summary>
    /// Referencia al documento de venta original que se devuelve. Recomendado.
    /// Máximo 30 caracteres. Ejemplo: "FAC-A-100"
    /// </summary>
    [StringLength(30, ErrorMessage = "La referencia no puede exceder 30 caracteres.")]
    public string ReferenciaDocumentoOrigen { get; set; } = string.Empty;

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
    /// Lista de productos devueltos. Se requiere al menos uno.
    /// </summary>
    [MinLength(1, ErrorMessage = "La nota de crédito debe tener al menos una partida.")]
    public List<NotaCreditoPartida> Partidas { get; set; } = new();
}

/// <summary>
/// Representa un producto devuelto dentro de una Nota de Crédito.
/// </summary>
public class NotaCreditoPartida
{
    /// <summary>
    /// Código del producto devuelto. Requerido. Máximo 30 caracteres.
    /// </summary>
    [Required(ErrorMessage = "El código del producto es requerido en cada partida.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código del producto debe tener entre 1 y 30 caracteres.")]
    public string CodigoProducto { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad de unidades devueltas. Debe ser mayor a 0.
    /// </summary>
    [Range(0.0001, double.MaxValue, ErrorMessage = "Las unidades deben ser mayores a 0.")]
    public double Unidades { get; set; }

    /// <summary>
    /// Precio unitario con el que se realizó la venta original. Debe ser mayor o igual a 0.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor o igual a 0.")]
    public double PrecioUnitario { get; set; }

    /// <summary>
    /// Código del almacén al que regresan los productos. Requerido. Máximo 30 caracteres.
    /// Ejemplo: "ALM01"
    /// </summary>
    [Required(ErrorMessage = "El código del almacén de regreso es requerido en cada partida.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código del almacén debe tener entre 1 y 30 caracteres.")]
    public string CodigoAlmacen { get; set; } = string.Empty;
}

public class CreateNotaCreditoCommandHandler : IRequestHandler<CreateNotaCreditoCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public CreateNotaCreditoCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(CreateNotaCreditoCommand request, CancellationToken cancellationToken)
    {
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
