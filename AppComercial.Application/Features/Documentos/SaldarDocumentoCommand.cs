using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Interfaces.SdkModels;
using MediatR;

using AppComercial.Application.Common.Services;
using System.Text.Json.Serialization;

namespace AppComercial.Application.Features.Documentos;

public class SaldarDocumentoCommand : IRequest<int>
{
    public string CodigoConceptoPagar { get; set; } = string.Empty;
    public string SeriePagar { get; set; } = string.Empty;
    public double FolioPagar { get; set; }
    
    public string CodigoConceptoPago { get; set; } = string.Empty;
    public string SeriePago { get; set; } = string.Empty;
    public double FolioPago { get; set; }

    /// <summary>
    /// Tipo de documento (CIDDOCUMENTODE) que debe tener el documento a pagar según el endpoint que invoca el comando; lo asigna el controlador.
    /// Si es null no se valida el tipo del concepto.
    /// </summary>
    [JsonIgnore]
    public int? TipoDocumentoEsperado { get; set; }
}

public class SaldarDocumentoCommandHandler : IRequestHandler<SaldarDocumentoCommand, int>
{
    private readonly IContpaqiSdk _sdk;
    private readonly IConceptoDocumentoValidator _conceptoValidator;

    public SaldarDocumentoCommandHandler(IContpaqiSdk sdk, IConceptoDocumentoValidator conceptoValidator)
    {
        _sdk = sdk;
        _conceptoValidator = conceptoValidator;
    }

    public async Task<int> Handle(SaldarDocumentoCommand request, CancellationToken cancellationToken)
    {
        if (request.TipoDocumentoEsperado.HasValue)
            await _conceptoValidator.ValidarAsync(request.CodigoConceptoPagar, request.TipoDocumentoEsperado.Value, cancellationToken);

        return await _sdk.SaldarDocumentoAsync(
            request.CodigoConceptoPagar, request.SeriePagar, request.FolioPagar,
            request.CodigoConceptoPago, request.SeriePago, request.FolioPago);
    }
}
