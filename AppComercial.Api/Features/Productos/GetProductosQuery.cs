using MediatR;
using AppComercial.Api.Models;
using AppComercial.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Api.Features.Productos;

public class GetProductosQuery : IRequest<IEnumerable<AdmProductos>>
{
    public string? Codigo { get; set; }
    public int? TipoProducto { get; set; } // Opcional: Para Filtrar Productos(1), Paquetes(2), Servicios(3)
}

public class GetProductosQueryHandler : IRequestHandler<GetProductosQuery, IEnumerable<AdmProductos>>
{
    private readonly ContpaqiDbContext _dbContext;

    public GetProductosQueryHandler(ContpaqiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<AdmProductos>> Handle(GetProductosQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Productos.AsNoTracking();

        if (!string.IsNullOrEmpty(request.Codigo))
        {
            query = query.Where(c => c.CCODIGOPRODUCTO == request.Codigo);
        }

        if (request.TipoProducto.HasValue)
        {
            query = query.Where(c => c.CTIPOPRODUCTO == request.TipoProducto.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }
}
