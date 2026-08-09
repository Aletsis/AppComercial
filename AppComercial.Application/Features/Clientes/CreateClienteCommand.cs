using MediatR;
using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Interfaces.SdkModels;
using AppComercial.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Application.Features.Clientes;

public class CreateClienteCommand : IRequest<int>
{
    public string Codigo { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string RFC { get; set; } = string.Empty;
    
    [System.Text.Json.Serialization.JsonIgnore]
    public int TipoCliente { get; set; } = 1; // 1 = Cliente
}

public class CreateClienteCommandHandler : IRequestHandler<CreateClienteCommand, int>
{
    private readonly IContpaqiSdk _sdk;
    private readonly IContpaqiDbContext _context;

    public CreateClienteCommandHandler(IContpaqiSdk sdk, IContpaqiDbContext context)
    {
        _sdk = sdk;
        _context = context;
    }

    public async Task<int> Handle(CreateClienteCommand request, CancellationToken cancellationToken)
    {
        var codigo = (request.Codigo ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ArgumentException("El código del cliente es requerido.");

        var razonSocial = (request.RazonSocial ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(razonSocial))
            throw new ArgumentException("La razón social del cliente es requerida.");

        var rfc = (request.RFC ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(rfc))
            rfc = "XAXX010101000";

        // Validar si el código ya existe
        var existe = await _context.Clientes.AnyAsync(c => c.CCODIGOCLIENTE == codigo, cancellationToken);
        if (existe)
            throw new Exception($"El código de cliente '{codigo}' ya existe en CONTPAQi.");

        var nuevoClienteParams = new tCteProv
        {
            cCodigoCliente = codigo,
            cRazonSocial = razonSocial,
            cRFC = rfc,
            cTipoCliente = request.TipoCliente == 0 ? 1 : request.TipoCliente
        };

        var nuevoId = await _sdk.CrearClienteAsync(nuevoClienteParams);
        return nuevoId;
    }
}
