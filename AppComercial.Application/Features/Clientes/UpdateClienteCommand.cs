using MediatR;
using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Interfaces.SdkModels;

namespace AppComercial.Application.Features.Clientes;

public class UpdateClienteCommand : IRequest<int>
{
    public string Codigo { get; set; } = string.Empty;
    public string? RazonSocial { get; set; }
    public string? RFC { get; set; }
    public string? RegimenFiscal { get; set; }
    public string? UsoCFDI { get; set; }
    
    [System.Text.Json.Serialization.JsonIgnore]
    public int TipoCliente { get; set; } = 1; // 1 = Cliente
}

public class UpdateClienteCommandHandler : IRequestHandler<UpdateClienteCommand, int>
{
    private readonly IContpaqiSdk _sdk;

    public UpdateClienteCommandHandler(IContpaqiSdk sdk)
    {
        _sdk = sdk;
    }

    public async Task<int> Handle(UpdateClienteCommand request, CancellationToken cancellationToken)
    {
        var codigo = (request.Codigo ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ArgumentException("El código del cliente es requerido.");

        var actualizarClienteParams = new Dictionary<string, string>();

        if (request.RazonSocial != null)
        {
            var razonSocial = request.RazonSocial.Trim();
            if (razonSocial.Length > SdkConstantes.kLongNombre - 1)
                razonSocial = razonSocial.Substring(0, SdkConstantes.kLongNombre - 1);
            actualizarClienteParams["CRAZONSOCIAL"] = razonSocial;
        }
            
        if (request.RFC != null)
        {
            var rfc = request.RFC.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(rfc))
                rfc = "XAXX010101000";
            if (rfc.Length > SdkConstantes.kLongRFC - 1)
                rfc = rfc.Substring(0, SdkConstantes.kLongRFC - 1);
            actualizarClienteParams["CRFC"] = rfc;
        }

        if (request.RegimenFiscal != null)
        {
            actualizarClienteParams["CREGIMFISC"] = request.RegimenFiscal.Trim();
        }

        if (request.UsoCFDI != null)
        {
            actualizarClienteParams["CUSOCFDI"] = request.UsoCFDI.Trim();
        }

        if (actualizarClienteParams.Count == 0)
        {
            throw new ArgumentException("No se proporcionaron datos para actualizar.");
        }

        var result = await _sdk.ActualizarClienteAsync(codigo, actualizarClienteParams);
        return result;
    }
}
