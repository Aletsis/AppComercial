using MediatR;
using AppComercial.Api.Models;
using AppComercial.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace AppComercial.Api.Features.Clientes;

public class GetClientesQuery : IRequest<IEnumerable<AdmClientes>>
{
    // Puedes agregar filtros aquí si es necesario
    public string? Codigo { get; set; }
    
    // Filtro para separar Clientes (1) / Clientes-Proveedores (2) / Proveedores (3)
    public int? TipoCliente { get; set; }
}

public class GetClientesQueryHandler : IRequestHandler<GetClientesQuery, IEnumerable<AdmClientes>>
{
    private readonly ContpaqiDbContext _dbContext;

    public GetClientesQueryHandler(ContpaqiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<AdmClientes>> Handle(GetClientesQuery request, CancellationToken cancellationToken)
    {
        // Consulta usando Entity Framework Core a la BD de SQL Server
        var query = _dbContext.Clientes.AsNoTracking();

        if (!string.IsNullOrEmpty(request.Codigo))
        {
            query = query.Where(c => c.CCODIGOCLIENTE == request.Codigo);
        }

        if (request.TipoCliente.HasValue)
        {
            query = query.Where(c => c.CTIPOCLIENTE == request.TipoCliente.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }
}
