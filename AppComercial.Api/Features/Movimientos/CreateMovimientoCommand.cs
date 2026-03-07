using MediatR;
using AppComercial.Api.Sdk;

namespace AppComercial.Api.Features.Movimientos;

public class CreateMovimientoCommand : IRequest<int>
{
    public int DocumentoId { get; set; }
    public double Unidades { get; set; }
    public double Precio { get; set; }
    public string CodigoProducto { get; set; } = string.Empty;
    public string CodigoAlmacen { get; set; } = "1"; // Default Almacen
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
            aConsecutivo = 0, // 0 = Asignar el siguiente automático
            aUnidades = request.Unidades,
            aPrecio = request.Precio,
            aCosto = 0, // Comercial SDK calcula si se omite, pero se recomienda poner el costo real para inventarios perpeutos si se tiene
            aCodProdSer = request.CodigoProducto,
            aCodAlmacen = request.CodigoAlmacen,
            aReferencia = string.Empty,
            aCodClasificacion = string.Empty
        };

        var result = await _sdk.CrearMovimientoAsync(request.DocumentoId, nuevoMovimiento);
        return result;
    }
}
