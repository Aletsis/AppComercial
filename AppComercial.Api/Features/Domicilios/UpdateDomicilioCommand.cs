using MediatR;
using AppComercial.Api.Sdk;

namespace AppComercial.Api.Features.Domicilios;

public class UpdateDomicilioCommand : IRequest<int>
{
    public int IdDireccion { get; set; }
    public Dictionary<string, string> Datos { get; set; } = new();
}

public class UpdateDomicilioCommandHandler : IRequestHandler<UpdateDomicilioCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public UpdateDomicilioCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(UpdateDomicilioCommand request, CancellationToken cancellationToken)
    {
        if (request.Datos == null || request.Datos.Count == 0)
            throw new ArgumentException("No se proporcionaron datos para actualizar.");

        return await _sdk.ActualizarDireccionAsync(request.IdDireccion, request.Datos);
    }
}
