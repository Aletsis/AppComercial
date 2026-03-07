using MediatR;
using AppComercial.Api.Sdk;

namespace AppComercial.Api.Features.Productos;

public class UpdateProductoCommand : IRequest<int>
{
    public string Codigo { get; set; } = string.Empty;
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
    public int? TipoProducto { get; set; }
    public int? ControlExistencia { get; set; }
}

public class UpdateProductoCommandHandler : IRequestHandler<UpdateProductoCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public UpdateProductoCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(UpdateProductoCommand request, CancellationToken cancellationToken)
    {
        var actualizarProductoParams = new Dictionary<string, string>();

        if (request.Nombre != null)
            actualizarProductoParams["CNOMBREPRODUCTO"] = request.Nombre;
            
        if (request.Descripcion != null)
            actualizarProductoParams["CDESCRIPCIONPRODUCTO"] = request.Descripcion;

        if (request.TipoProducto != null)
            actualizarProductoParams["CTIPOPRODUCTO"] = request.TipoProducto.Value.ToString();

        if (request.ControlExistencia != null)
            actualizarProductoParams["CCONTROLEXISTENCIA"] = request.ControlExistencia.Value.ToString();

        if (actualizarProductoParams.Count == 0)
        {
            throw new ArgumentException("No se proporcionaron datos para actualizar.");
        }

        var result = await _sdk.ActualizarProductoAsync(request.Codigo, actualizarProductoParams);
        return result;
    }
}
