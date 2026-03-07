using MediatR;
using AppComercial.Api.Sdk;

namespace AppComercial.Api.Features.Documentos;

public class UpdateDocumentoCommand : IRequest<int>
{
    public string CodigoConcepto { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public string Folio { get; set; } = string.Empty;
    public Dictionary<string, string> Datos { get; set; } = new();
}

public class UpdateDocumentoCommandHandler : IRequestHandler<UpdateDocumentoCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public UpdateDocumentoCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(UpdateDocumentoCommand request, CancellationToken cancellationToken)
    {
        if (request.Datos == null || request.Datos.Count == 0)
            throw new ArgumentException("No se proporcionaron datos para actualizar.");

        return await _sdk.ActualizarDocumentoAsync(request.CodigoConcepto, request.Serie, request.Folio, request.Datos);
    }
}
