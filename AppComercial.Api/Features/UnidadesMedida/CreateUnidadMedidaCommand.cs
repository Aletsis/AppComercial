using AppComercial.Api.Sdk;
using MediatR;

namespace AppComercial.Api.Features.UnidadesMedida;

public class CreateUnidadMedidaCommand : IRequest<int>
{
    public string NombreUnidad { get; set; } = string.Empty;
    public string Abreviatura { get; set; } = string.Empty;
    public string Despliegue { get; set; } = string.Empty;
}

public class CreateUnidadMedidaCommandHandler : IRequestHandler<CreateUnidadMedidaCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public CreateUnidadMedidaCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(CreateUnidadMedidaCommand request, CancellationToken cancellationToken)
    {
        var nuevaUnidad = new tUnidad
        {
            cNombreUnidad = request.NombreUnidad,
            cAbreviatura = request.Abreviatura,
            cDespliegue = request.Despliegue
        };

        return await _sdk.CrearUnidadMedidaAsync(nuevaUnidad);
    }
}
