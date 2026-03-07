using MediatR;
using AppComercial.Api.Sdk;

namespace AppComercial.Api.Features.Movimientos;

public class UpdateMovimientoCommand : IRequest<int>
{
    public int IdMovimiento { get; set; }
    public Dictionary<string, string> Datos { get; set; } = new();
}

public class UpdateMovimientoCommandHandler : IRequestHandler<UpdateMovimientoCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public UpdateMovimientoCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(UpdateMovimientoCommand request, CancellationToken cancellationToken)
    {
        if (request.Datos == null || request.Datos.Count == 0)
            throw new ArgumentException("No se proporcionaron datos para actualizar.");

        return await _sdk.ActualizarMovimientoAsync(request.IdMovimiento, request.Datos);
    }
}
