using MediatR;
using AppComercial.Api.Models;
using AppComercial.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Api.Features.Movimientos;

public class GetMovimientosQuery : IRequest<IEnumerable<AdmMovimientos>>
{
    public int? DocumentoId { get; set; }
    public int? ProductoId { get; set; }
}

public class GetMovimientosQueryHandler : IRequestHandler<GetMovimientosQuery, IEnumerable<AdmMovimientos>>
{
    private readonly ContpaqiDbContext _dbContext;

    public GetMovimientosQueryHandler(ContpaqiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<AdmMovimientos>> Handle(GetMovimientosQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Movimientos.AsNoTracking();

        if (request.DocumentoId.HasValue)
        {
            query = query.Where(m => m.CIDDOCUMENTO == request.DocumentoId.Value);
        }

        if (request.ProductoId.HasValue)
        {
            query = query.Where(m => m.CIDPRODUCTO == request.ProductoId.Value);
        }

        // Recomendación: Limitar la consulta para que no traiga cien mil renglones si no le pasan ningún filtro
        if (!request.DocumentoId.HasValue && !request.ProductoId.HasValue)
        {
            query = query.OrderByDescending(m => m.CIDMOVIMIENTO).Take(500); 
        }

        return await query.ToListAsync(cancellationToken);
    }
}
