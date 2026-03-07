using MediatR;
using AppComercial.Api.Sdk;

namespace AppComercial.Api.Features.Productos;

public class CreateProductoCommand : IRequest<int>
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int TipoProducto { get; set; } = 1; // 1 = Producto, 2 = Paquete, 3 = Servicio
    public int ControlExistencia { get; set; } = 0; // 0 = Ninguno
}

public class CreateProductoCommandHandler : IRequestHandler<CreateProductoCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public CreateProductoCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(CreateProductoCommand request, CancellationToken cancellationToken)
    {
        var nuevoProductoParams = new tProducto
        {
            cCodigoProducto = request.Codigo,
            cNombreProducto = request.Nombre,
            cDescripcionProducto = request.Descripcion,
            cTipoProducto = request.TipoProducto,
            cControlExistencia = request.ControlExistencia
        };

        var nuevoId = await _sdk.CrearProductoAsync(nuevoProductoParams);
        return nuevoId;
    }
}
