using MediatR;
using AppComercial.Api.Sdk;

namespace AppComercial.Api.Features.UnidadesMedida;

public class UpdateUnidadMedidaCommand : IRequest<int>
{
    public string NombreUnidad { get; set; } = string.Empty;
    public Dictionary<string, string> Datos { get; set; } = new();
}

public class UpdateUnidadMedidaCommandHandler : IRequestHandler<UpdateUnidadMedidaCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public UpdateUnidadMedidaCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(UpdateUnidadMedidaCommand request, CancellationToken cancellationToken)
    {
        if (request.Datos == null || request.Datos.Count == 0)
            throw new ArgumentException("No se proporcionaron datos para actualizar.");

        return await _sdk.ActualizarUnidadMedidaAsync(request.NombreUnidad, request.Datos);
    }
}
