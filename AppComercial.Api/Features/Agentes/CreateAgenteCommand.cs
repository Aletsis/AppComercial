using MediatR;
using AppComercial.Api.Sdk;

namespace AppComercial.Api.Features.Agentes;

public class CreateAgenteCommand : IRequest<int>
{
    public Dictionary<string, string> Datos { get; set; } = new();
}

public class CreateAgenteCommandHandler : IRequestHandler<CreateAgenteCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public CreateAgenteCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(CreateAgenteCommand request, CancellationToken cancellationToken)
    {
        return await _sdk.CrearAgenteAsync(request.Datos);
    }
}
