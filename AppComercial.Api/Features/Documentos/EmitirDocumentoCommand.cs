using AppComercial.Api.Sdk;
using MediatR;

namespace AppComercial.Api.Features.Documentos;

public class EmitirDocumentoCommand : IRequest<int>
{
    public string CodigoConcepto { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public double Folio { get; set; }
    public string PasswordContpaqi { get; set; } = string.Empty;
    public string CorreoOArchivoAdicional { get; set; } = string.Empty; // En Comercial puede separar correos por ; o ruta de visor PFD
}

public class EmitirDocumentoCommandHandler : IRequestHandler<EmitirDocumentoCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public EmitirDocumentoCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(EmitirDocumentoCommand request, CancellationToken cancellationToken)
    {
        return await _sdk.EmitirDocumentoAsync(request.CodigoConcepto, request.Serie, request.Folio, request.PasswordContpaqi, request.CorreoOArchivoAdicional);
    }
}
