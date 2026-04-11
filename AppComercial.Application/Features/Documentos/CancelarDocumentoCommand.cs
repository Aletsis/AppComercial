using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Interfaces.SdkModels;
using MediatR;

namespace AppComercial.Application.Features.Documentos;

public class CancelarDocumentoCommand : IRequest<int>
{
    public string CodigoConcepto { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public double Folio { get; set; }
    public string PasswordContpaqi { get; set; } = string.Empty;
}

public class CancelarDocumentoCommandHandler : IRequestHandler<CancelarDocumentoCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public CancelarDocumentoCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(CancelarDocumentoCommand request, CancellationToken cancellationToken)
    {
        return await _sdk.CancelarDocumentoAsync(request.CodigoConcepto, request.Serie, request.Folio, request.PasswordContpaqi);
    }
}
