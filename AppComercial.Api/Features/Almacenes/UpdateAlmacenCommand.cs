using MediatR;
using AppComercial.Api.Sdk;

namespace AppComercial.Api.Features.Almacenes;

public class UpdateAlmacenCommand : IRequest<int>
{
    public string Codigo { get; set; } = string.Empty;
    public Dictionary<string, string> Datos { get; set; } = new();
}

public class UpdateAlmacenCommandHandler : IRequestHandler<UpdateAlmacenCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public UpdateAlmacenCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(UpdateAlmacenCommand request, CancellationToken cancellationToken)
    {
        if (request.Datos == null || request.Datos.Count == 0)
            throw new ArgumentException("No se proporcionaron datos para actualizar.");

        return await _sdk.ActualizarAlmacenAsync(request.Codigo, request.Datos);
    }
}
