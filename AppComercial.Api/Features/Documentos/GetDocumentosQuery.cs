using MediatR;
using AppComercial.Api.Models;
using AppComercial.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Api.Features.Documentos;

public class GetDocumentosQuery : IRequest<IEnumerable<AdmDocumentos>>
{
    public int? ConceptoDocumentoId { get; set; }
    public string? Serie { get; set; }
    public double? Folio { get; set; }
    public int? ClienteProveedorId { get; set; }
}

public class GetDocumentosQueryHandler : IRequestHandler<GetDocumentosQuery, IEnumerable<AdmDocumentos>>
{
    private readonly ContpaqiDbContext _dbContext;

    public GetDocumentosQueryHandler(ContpaqiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<AdmDocumentos>> Handle(GetDocumentosQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Documentos.AsNoTracking();

        if (request.ConceptoDocumentoId.HasValue)
        {
            query = query.Where(d => d.CIDCONCEPTODOCUMENTO == request.ConceptoDocumentoId.Value);
        }

        if (!string.IsNullOrEmpty(request.Serie))
        {
            query = query.Where(d => d.CSERIEDOCUMENTO == request.Serie);
        }

        if (request.Folio.HasValue)
        {
            query = query.Where(d => d.CFOLIO == request.Folio.Value);
        }

        if (request.ClienteProveedorId.HasValue)
        {
            query = query.Where(d => d.CIDCLIENTEPROVEEDOR == request.ClienteProveedorId.Value);
        }

        // Recomendación: Limitar la cantidad de registros por defecto ya que las facturas crecen mucho
        return await query.OrderByDescending(d => d.CFECHA).Take(100).ToListAsync(cancellationToken);
    }
}
