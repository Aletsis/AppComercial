using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Interfaces.SdkModels;
using MediatR;

namespace AppComercial.Application.Features.Documentos;

public class SaldarDocumentoCommand : IRequest<int>
{
    public string CodigoConceptoPagar { get; set; } = string.Empty;
    public string SeriePagar { get; set; } = string.Empty;
    public double FolioPagar { get; set; }
    
    public string CodigoConceptoPago { get; set; } = string.Empty;
    public string SeriePago { get; set; } = string.Empty;
    public double FolioPago { get; set; }
}

public class SaldarDocumentoCommandHandler : IRequestHandler<SaldarDocumentoCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public SaldarDocumentoCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(SaldarDocumentoCommand request, CancellationToken cancellationToken)
    {
        return await _sdk.SaldarDocumentoAsync(
            request.CodigoConceptoPagar, request.SeriePagar, request.FolioPagar,
            request.CodigoConceptoPago, request.SeriePago, request.FolioPago);
    }
}
