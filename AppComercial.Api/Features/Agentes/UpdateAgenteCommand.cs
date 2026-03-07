using MediatR;
using AppComercial.Api.Sdk;

namespace AppComercial.Api.Features.Agentes;

public class UpdateAgenteCommand : IRequest<int>
{
    public string Codigo { get; set; } = string.Empty;
    public Dictionary<string, string> Datos { get; set; } = new();
}

public class UpdateAgenteCommandHandler : IRequestHandler<UpdateAgenteCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public UpdateAgenteCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(UpdateAgenteCommand request, CancellationToken cancellationToken)
    {
        if (request.Datos == null || request.Datos.Count == 0)
        {
            throw new ArgumentException("No se proporcionaron datos para actualizar.");
        }
        return await _sdk.ActualizarAgenteAsync(request.Codigo, request.Datos);
    }
}
