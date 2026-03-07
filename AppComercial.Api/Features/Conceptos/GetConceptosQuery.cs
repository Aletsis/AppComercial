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
        var query = _dbContext.Conceptos.AsNoTracking();

        if (!string.IsNullOrEmpty(request.Codigo))
        {
            query = query.Where(c => c.CCODIGOCONCEPTO == request.Codigo);
        }

        if (request.TipoDocumento.HasValue)
        {
            query = query.Where(c => c.CIDDOCUMENTODE == request.TipoDocumento.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }
}
