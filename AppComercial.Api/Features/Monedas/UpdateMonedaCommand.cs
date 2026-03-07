using MediatR;
using AppComercial.Api.Sdk;

namespace AppComercial.Api.Features.Monedas;

public class UpdateMonedaCommand : IRequest<int>
{
    public int IdMoneda { get; set; }
    public Dictionary<string, string> Datos { get; set; } = new();
}

public class UpdateMonedaCommandHandler : IRequestHandler<UpdateMonedaCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public UpdateMonedaCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(UpdateMonedaCommand request, CancellationToken cancellationToken)
    {
        if (request.Datos == null || request.Datos.Count == 0)
            throw new ArgumentException("No se proporcionaron datos para actualizar.");

        return await _sdk.ActualizarMonedaAsync(request.IdMoneda, request.Datos);
    }
}
