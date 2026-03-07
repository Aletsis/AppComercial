using MediatR;
using AppComercial.Api.Sdk;

namespace AppComercial.Api.Features.Conceptos;

public class CreateConceptoCommand : IRequest<int>
{
    public Dictionary<string, string> Datos { get; set; } = new();
}

public class CreateConceptoCommandHandler : IRequestHandler<CreateConceptoCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public CreateConceptoCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(CreateConceptoCommand request, CancellationToken cancellationToken)
    {
        return await _sdk.CrearConceptoAsync(request.Datos);
    }
}
