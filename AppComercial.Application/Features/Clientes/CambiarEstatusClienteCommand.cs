using MediatR;
using AppComercial.Domain.Interfaces;
using AppComercial.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AppComercial.Application.Features.Clientes;

public class CambiarEstatusClienteCommand : IRequest<bool>
{
    public string Codigo { get; set; } = string.Empty;
    public bool Activo { get; set; }

    public CambiarEstatusClienteCommand() { }

    public CambiarEstatusClienteCommand(string codigo, bool activo)
    {
        Codigo = codigo;
        Activo = activo;
    }
}

public class CambiarEstatusClienteCommandHandler : IRequestHandler<CambiarEstatusClienteCommand, bool>
{
    private readonly IContpaqiSdk _sdk;
    private readonly IContpaqiDbContext _context;

    public CambiarEstatusClienteCommandHandler(IContpaqiSdk sdk, IContpaqiDbContext context)
    {
        _sdk = sdk;
        _context = context;
    }

    public async Task<bool> Handle(CambiarEstatusClienteCommand request, CancellationToken cancellationToken)
    {
        var codigo = (request.Codigo ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ArgumentException("El código del cliente es requerido.");

        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.CCODIGOCLIENTE == codigo, cancellationToken);
        if (cliente == null)
            throw new Exception($"El cliente con código '{codigo}' no existe en CONTPAQi.");

        var actualizarParams = new Dictionary<string, string>
        {
            ["CESTATUS"] = request.Activo ? "1" : "0",
            ["CFECHABAJA"] = request.Activo ? string.Empty : DateTime.Now.ToString("MM/dd/yyyy")
        };

        var result = await _sdk.ActualizarClienteAsync(codigo, actualizarParams);
        return result == 0;
    }
}
