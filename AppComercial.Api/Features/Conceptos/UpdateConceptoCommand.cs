using MediatR;
using AppComercial.Api.Sdk;

namespace AppComercial.Api.Features.Conceptos;

public class UpdateConceptoCommand : IRequest<int>
{
    public string Codigo { get; set; } = string.Empty;
    public Dictionary<string, string> Datos { get; set; } = new();
}

public class UpdateConceptoCommandHandler : IRequestHandler<UpdateConceptoCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public UpdateConceptoCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(UpdateConceptoCommand request, CancellationToken cancellationToken)
    {
        if (request.Datos == null || request.Datos.Count == 0)
            throw new ArgumentException("No se proporcionaron datos para actualizar.");

        return await _sdk.ActualizarConceptoAsync(request.Codigo, request.Datos);
    }
}
