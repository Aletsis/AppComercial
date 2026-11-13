using MediatR;
using AppComercial.Domain.Interfaces;
using AppComercial.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AppComercial.Application.Features.Proveedores;

public class CambiarEstatusProveedorCommand : IRequest<bool>
{
    public string Codigo { get; set; } = string.Empty;
    public bool Activo { get; set; }

    public CambiarEstatusProveedorCommand() { }

    public CambiarEstatusProveedorCommand(string codigo, bool activo)
    {
        Codigo = codigo;
        Activo = activo;
    }
}

public class CambiarEstatusProveedorCommandHandler : IRequestHandler<CambiarEstatusProveedorCommand, bool>
{
    private readonly IContpaqiSdk _sdk;
    private readonly IContpaqiDbContext _context;

    public CambiarEstatusProveedorCommandHandler(IContpaqiSdk sdk, IContpaqiDbContext context)
    {
        _sdk = sdk;
        _context = context;
    }

    public async Task<bool> Handle(CambiarEstatusProveedorCommand request, CancellationToken cancellationToken)
    {
        var codigo = (request.Codigo ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ArgumentException("El código del proveedor es requerido.");

        var proveedor = await _context.Clientes.FirstOrDefaultAsync(c => c.CCODIGOCLIENTE == codigo, cancellationToken);
        if (proveedor == null)
            throw new Exception($"El proveedor con código '{codigo}' no existe en CONTPAQi.");

        var actualizarParams = new Dictionary<string, string>
        {
            ["CESTATUS"] = request.Activo ? "1" : "0",
            ["CFECHABAJA"] = request.Activo ? string.Empty : DateTime.Now.ToString("MM/dd/yyyy")
        };

        var result = await _sdk.ActualizarClienteAsync(codigo, actualizarParams);
        return result == 0;
    }
}
