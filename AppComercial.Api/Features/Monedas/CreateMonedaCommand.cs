using MediatR;
using AppComercial.Api.Sdk;

namespace AppComercial.Api.Features.Monedas;

public class CreateMonedaCommand : IRequest<int>
{
    public Dictionary<string, string> Datos { get; set; } = new();
}

public class CreateMonedaCommandHandler : IRequestHandler<CreateMonedaCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public CreateMonedaCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(CreateMonedaCommand request, CancellationToken cancellationToken)
    {
        return await _sdk.CrearMonedaAsync(request.Datos);
    }
}
