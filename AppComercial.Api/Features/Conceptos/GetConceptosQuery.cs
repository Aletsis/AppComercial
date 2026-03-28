using MediatR;
using AppComercial.Api.Models;
using AppComercial.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Api.Features.Conceptos;

public class GetConceptosQuery : IRequest<IEnumerable<AdmConceptos>>
{
    public string? Codigo { get; set; }
    public int? TipoDocumento { get; set; } // Representa el tipo de documento al que pertenece (ej. Facturas, Compras, etc.)
}

public class GetConceptosQueryHandler : IRequestHandler<GetConceptosQuery, IEnumerable<AdmConceptos>>
{
    private readonly ContpaqiDbContext _dbContext;

    public GetConceptosQueryHandler(ContpaqiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<AdmConceptos>> Handle(GetConceptosQuery request, CancellationToken cancellationToken)
    {
        var query = from c in _dbContext.Conceptos.AsNoTracking()
                    join a in _dbContext.Almacenes.AsNoTracking() on c.CIDALMASUM equals a.CIDALMACEN into joined
                    from al in joined.DefaultIfEmpty()
                    select new { c, CodigoAlm = al != null ? al.CCODIGOALMACEN : "" };

        if (!string.IsNullOrEmpty(request.Codigo))
        {
            query = query.Where(x => x.c.CCODIGOCONCEPTO == request.Codigo);
        }

        if (request.TipoDocumento.HasValue)
        {
            query = query.Where(x => x.c.CIDDOCUMENTODE == request.TipoDocumento.Value);
        }

        var results = await query.ToListAsync(cancellationToken);
        
        foreach (var item in results)
        {
            item.c.CCODIGOALMACEN = item.CodigoAlm;
        }

        return results.Select(x => x.c);
    }
}
