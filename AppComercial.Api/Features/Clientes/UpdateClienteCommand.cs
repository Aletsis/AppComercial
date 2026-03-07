using MediatR;
using AppComercial.Api.Sdk;

namespace AppComercial.Api.Features.Clientes;

public class UpdateClienteCommand : IRequest<int>
{
    public string Codigo { get; set; } = string.Empty;
    public string? RazonSocial { get; set; }
    public string? RFC { get; set; }
    
    [System.Text.Json.Serialization.JsonIgnore]
    public int TipoCliente { get; set; } = 1; // 1 = Cliente
}

public class UpdateClienteCommandHandler : IRequestHandler<UpdateClienteCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public UpdateClienteCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(UpdateClienteCommand request, CancellationToken cancellationToken)
    {
        var actualizarClienteParams = new Dictionary<string, string>();

        if (request.RazonSocial != null)
            actualizarClienteParams["CRAZONSOCIAL"] = request.RazonSocial;
            
        if (request.RFC != null)
            actualizarClienteParams["CRFC"] = request.RFC;

        // Tipo de cliente no se suele actualizar una vez creado, pero si es necesario:
        // actualizarClienteParams["CTIPOCLIENTE"] = request.TipoCliente.ToString();

        if (actualizarClienteParams.Count == 0)
        {
            throw new ArgumentException("No se proporcionaron datos para actualizar.");
        }

        var result = await _sdk.ActualizarClienteAsync(request.Codigo, actualizarClienteParams);
        return result;
    }
}
