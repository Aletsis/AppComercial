using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Interfaces.SdkModels;
using MediatR;

using AppComercial.Application.Common.Services;
using System.Text.Json.Serialization;

namespace AppComercial.Application.Features.Documentos;

public class CancelarDocumentoCommand : IRequest<int>
{
    public string CodigoConcepto { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public double Folio { get; set; }
    public string PasswordContpaqi { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de documento (CIDDOCUMENTODE) que exige el endpoint que invoca el comando; lo asigna el controlador.
    /// Si es null no se valida el tipo del concepto.
    /// </summary>
    [JsonIgnore]
    public int? TipoDocumentoEsperado { get; set; }
}

public class CancelarDocumentoCommandHandler : IRequestHandler<CancelarDocumentoCommand, int>
{
    private readonly IContpaqiSdk _sdk;
    private readonly IConceptoDocumentoValidator _conceptoValidator;

    public CancelarDocumentoCommandHandler(IContpaqiSdk sdk, IConceptoDocumentoValidator conceptoValidator)
    {
        _sdk = sdk;
        _conceptoValidator = conceptoValidator;
    }

    public async Task<int> Handle(CancelarDocumentoCommand request, CancellationToken cancellationToken)
    {
        if (request.TipoDocumentoEsperado.HasValue)
            await _conceptoValidator.ValidarAsync(request.CodigoConcepto, request.TipoDocumentoEsperado.Value, cancellationToken);

        return await _sdk.CancelarDocumentoAsync(request.CodigoConcepto, request.Serie, request.Folio, request.PasswordContpaqi);
    }
}
