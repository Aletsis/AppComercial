using MediatR;
using AppComercial.Api.Sdk;

namespace AppComercial.Api.Features.Documentos;

public class CreateDocumentoCommand : IRequest<int>
{
    public string Concepto { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public string CodigoClienteProveedor { get; set; } = string.Empty;
    public string Referencia { get; set; } = string.Empty;
}

public class CreateDocumentoCommandHandler : IRequestHandler<CreateDocumentoCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public CreateDocumentoCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(CreateDocumentoCommand request, CancellationToken cancellationToken)
    {
        var nuevoDocumento = new tDocumento
        {
            aCodConcepto = request.Concepto,
            aSerie = request.Serie,
            aCodigoCteProv = request.CodigoClienteProveedor,
            aReferencia = request.Referencia,
            aFecha = DateTime.Now.ToString("MM/dd/yyyy"),
            aNumMoneda = 1, // Peso Mexicano default
            aTipoCambio = 1.0
        };

        var result = await _sdk.CrearDocumentoAsync(nuevoDocumento);
        return result;
    }
}
