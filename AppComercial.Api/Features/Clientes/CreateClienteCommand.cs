using MediatR;
using AppComercial.Api.Sdk;

namespace AppComercial.Api.Features.Clientes;

public class CreateClienteCommand : IRequest<int>
{
    public string Codigo { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string RFC { get; set; } = string.Empty;
    
    [System.Text.Json.Serialization.JsonIgnore]
    public int TipoCliente { get; set; } = 1; // 1 = Cliente
}

public class CreateClienteCommandHandler : IRequestHandler<CreateClienteCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public CreateClienteCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(CreateClienteCommand request, CancellationToken cancellationToken)
    {
        var nuevoClienteParams = new tCteProv
        {
            cCodigoCliente = request.Codigo,
            cRazonSocial = request.RazonSocial,
            cRFC = request.RFC,
            cTipoCliente = request.TipoCliente
        };

        var nuevoId = await _sdk.CrearClienteAsync(nuevoClienteParams);
        return nuevoId;
    }
}
