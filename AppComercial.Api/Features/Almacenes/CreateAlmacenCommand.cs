using MediatR;
using AppComercial.Api.Sdk;

namespace AppComercial.Api.Features.Almacenes;

public class CreateAlmacenCommand : IRequest<int>
{
    public Dictionary<string, string> Datos { get; set; } = new();
}

public class CreateAlmacenCommandHandler : IRequestHandler<CreateAlmacenCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public CreateAlmacenCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(CreateAlmacenCommand request, CancellationToken cancellationToken)
    {
        return await _sdk.CrearAlmacenAsync(request.Datos);
    }
}
