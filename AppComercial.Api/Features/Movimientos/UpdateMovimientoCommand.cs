using MediatR;
using AppComercial.Api.Sdk;
using AppComercial.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AppComercial.Api.Features.Movimientos;

/// <summary>
/// Comando para actualizar campos de una Partida (Movimiento) existente en CONTPAQi Comercial.
/// El ID del movimiento se proporciona en la URL (ruta) del endpoint PUT.
/// Al menos uno de los campos opcionales debe ser enviado.
/// </summary>
public class UpdateMovimientoCommand : IRequest<int>
{
    /// <summary>
    /// ID interno del movimiento a actualizar. Se asigna automáticamente desde la URL.
    /// No es necesario incluirlo en el cuerpo del request.
    /// </summary>
    public int IdMovimiento { get; set; }

    /// <summary>
    /// Nuevo código del producto o servicio. Opcional. Máximo 30 caracteres.
    /// </summary>
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código del producto debe tener entre 1 y 30 caracteres.")]
    public string? CodigoProducto { get; set; }

    /// <summary>
    /// Nueva cantidad de unidades. Opcional. Debe ser mayor a 0.
    /// </summary>
    [Range(0.0001, double.MaxValue, ErrorMessage = "Las unidades deben ser mayores a 0.")]
    public double? Unidades { get; set; }

    /// <summary>
    /// Nuevo precio unitario de venta (sin impuestos). Opcional. Debe ser mayor o igual a 0.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser mayor o igual a 0.")]
    public double? Precio { get; set; }

    /// <summary>
    /// Nuevo costo unitario del producto. Opcional. Debe ser mayor o igual a 0.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "El costo debe ser mayor o igual a 0.")]
    public double? Costo { get; set; }

    /// <summary>
    /// Nuevo código de almacén. Opcional. Máximo 30 caracteres.
    /// </summary>
    [StringLength(30, MinimumLength = 1, ErrorMessage = "El código del almacén debe tener entre 1 y 30 caracteres.")]
    public string? CodigoAlmacen { get; set; }

    /// <summary>
    /// Nueva referencia de la partida. Opcional. Máximo 60 caracteres.
    /// </summary>
    [StringLength(60, ErrorMessage = "La referencia no puede exceder 60 caracteres.")]
    public string? Referencia { get; set; }
}

public class UpdateMovimientoCommandHandler : IRequestHandler<UpdateMovimientoCommand, int>
{
    private readonly IContpaqiSdk _sdk;
    private readonly ContpaqiDbContext _context;

    public UpdateMovimientoCommandHandler(IContpaqiSdk sdk, ContpaqiDbContext context)
    {
        _sdk = sdk;
        _context = context;
    }

    public async Task<int> Handle(UpdateMovimientoCommand request, CancellationToken cancellationToken)
    {
        var dbMovimiento = await _context.Movimientos.FirstOrDefaultAsync(m => m.CIDMOVIMIENTO == request.IdMovimiento, cancellationToken);
        if (dbMovimiento == null) throw new KeyNotFoundException($"Movimiento con ID {request.IdMovimiento} no encontrado.");

        var datos = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(request.CodigoProducto))
            datos["PRODUCTO"] = request.CodigoProducto;

        if (request.Unidades.HasValue)
            datos["UNIDADES"] = request.Unidades.Value.ToString("F6");

        if (request.Precio.HasValue)
            datos["PRECIO"] = request.Precio.Value.ToString("F6");

        if (request.Costo.HasValue)
            datos["COSTO"] = request.Costo.Value.ToString("F6");

        if (!string.IsNullOrWhiteSpace(request.CodigoAlmacen))
            datos["ALMACEN"] = request.CodigoAlmacen;

        if (!string.IsNullOrWhiteSpace(request.Referencia))
            datos["REFERENCIA"] = request.Referencia;

        if (datos.Count == 0)
            throw new ArgumentException(
                "Se debe proporcionar al menos un campo para actualizar: " +
                "CodigoProducto, Unidades, Precio, Costo, CodigoAlmacen o Referencia.");

        return await _sdk.ActualizarMovimientoAsync(dbMovimiento.CIDDOCUMENTO, request.IdMovimiento, datos);
    }
}
