using AppComercial.Api.Sdk;
using MediatR;

namespace AppComercial.Api.Features.Domicilios;

public class CreateDomicilioCommand : IRequest<int>
{
    public string CodigoCatalogo { get; set; } = string.Empty;
    public int TipoCatalogo { get; set; } // 1 = Cliente, 2 = Proveedor
    public int TipoDireccion { get; set; } // 0 = Fiscal, 1 = Envio
    public string Calle { get; set; } = string.Empty;
    public string NumeroExterior { get; set; } = string.Empty;
    public string NumeroInterior { get; set; } = string.Empty;
    public string Colonia { get; set; } = string.Empty;
    public string CodigoPostal { get; set; } = string.Empty;
    public string Telefono1 { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Pais { get; set; } = "México";
}

public class CreateDomicilioCommandHandler : IRequestHandler<CreateDomicilioCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public CreateDomicilioCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(CreateDomicilioCommand request, CancellationToken cancellationToken)
    {
        var nuevaDireccion = new tDireccion
        {
            cCodCatalogo = request.CodigoCatalogo,
            cTipoCatalogo = request.TipoCatalogo,
            cTipoDireccion = request.TipoDireccion,
            cNombreCalle = request.Calle,
            cNumeroExterior = request.NumeroExterior,
            cNumeroInterior = request.NumeroInterior,
            cColonia = request.Colonia,
            cCodigoPostal = request.CodigoPostal,
            cTelefono1 = request.Telefono1,
            cEmail = request.Email,
            cCiudad = request.Ciudad,
            cEstado = request.Estado,
            cPais = request.Pais,
            // Valores por omisión para cumplir SDK lengths
            cTelefono2 = string.Empty,
            cTelefono3 = string.Empty,
            cTelefono4 = string.Empty,
            cDireccionWeb = string.Empty,
            cTextoExtra = string.Empty
        };

        return await _sdk.CrearDireccionAsync(nuevaDireccion);
    }
}
