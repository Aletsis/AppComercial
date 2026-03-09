using MediatR;
using AppComercial.Api.Sdk;
using System.ComponentModel.DataAnnotations;

namespace AppComercial.Api.Features.Movimientos;

/// <summary>
/// Comando para agregar una nueva Partida (Movimiento) a un Documento existente en CONTPAQi Comercial.
/// El documento debe haber sido creado previamente con POST /api/Documentos.
/// </summary>
public class CreateMovimientoCommand : IRequest<int>
{
    /// <summary>
    /// ID interno del documento al que se agrega la partida. Requerido.
    /// Este ID es el retornado al crear el documento con POST /api/Documentos.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "El ID del documento debe ser mayor a 0.")]
    public int DocumentoId { get; set; }

    /// <summary>
    /// Código del producto o servicio de la partida. Requerido. Máximo 30 caracteres.
    /// Ejemplo: "PROD-001"
    /// </summary>
    [Required(ErrorMessage = "El código del producto es requerido.")]
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
    [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser mayor o igual a 0.")]
    public double Precio { get; set; }

    /// <summary>
    /// Costo unitario del producto. Opcional, por defecto 0.
    /// Se recomienda especificarlo en inventarios perpetuos para un costeo correcto.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "El costo debe ser mayor o igual a 0.")]
    public double Costo { get; set; } = 0;

    /// <summary>
    /// Código del almacén de donde sale o entra el producto. Requerido. Máximo 30 caracteres.
    /// Ejemplo: "ALM01"
    /// </summary>
    [Required(ErrorMessage = "El código del almacén es requerido.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código del almacén debe tener entre 1 y 30 caracteres.")]
    public string CodigoAlmacen { get; set; } = string.Empty;

    /// <summary>
    /// Referencia adicional de la partida. Opcional. Máximo 60 caracteres.
    /// </summary>
    [StringLength(60, ErrorMessage = "La referencia no puede exceder 60 caracteres.")]
    public string Referencia { get; set; } = string.Empty;
}

public class CreateMovimientoCommandHandler : IRequestHandler<CreateMovimientoCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public CreateMovimientoCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(CreateMovimientoCommand request, CancellationToken cancellationToken)
    {
        var nuevoMovimiento = new tMovimiento
        {
            aConsecutivo      = 0, // 0 = asignar el siguiente consecutivo automáticamente
            aCodProdSer       = request.CodigoProducto,
            aUnidades         = request.Unidades,
            aPrecio           = request.Precio,
            aCosto            = request.Costo,
            aCodAlmacen       = request.CodigoAlmacen,
            aReferencia       = request.Referencia,
            aCodClasificacion = string.Empty
        };

        return await _sdk.CrearMovimientoAsync(request.DocumentoId, nuevoMovimiento);
    }
}
