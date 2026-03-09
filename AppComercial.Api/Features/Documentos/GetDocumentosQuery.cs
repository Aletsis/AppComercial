using MediatR;
using AppComercial.Api.Models;
using AppComercial.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Api.Features.Documentos;

public class GetDocumentosQuery : IRequest<IEnumerable<AdmDocumentos>>
{
    /// <summary>Filtra por ID interno del concepto (opcional, uso interno).</summary>
    public int? ConceptoDocumentoId { get; set; }

    /// <summary>Filtra por código de concepto (ej. "FAC", "COMP"). Más intuitivo que el ID.</summary>
    public string? CodigoConcepto { get; set; }

    /// <summary>Filtra por naturaleza del documento: 1=Venta, 2=Compra, 3=DevVenta, 4=EntAlmacen, 5=SalAlmacen.</summary>
    public int? Naturaleza { get; set; }

    public string? Serie { get; set; }
    public double? Folio { get; set; }
    public int? ClienteProveedorId { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }

    /// <summary>Máximo de registros a retornar. Por defecto: 100.</summary>
    public int Take { get; set; } = 100;
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

        // Filtros directos en el documento
        if (request.ConceptoDocumentoId.HasValue)
            query = query.Where(d => d.CIDCONCEPTODOCUMENTO == request.ConceptoDocumentoId.Value);

        if (!string.IsNullOrWhiteSpace(request.Serie))
            query = query.Where(d => d.CSERIEDOCUMENTO == request.Serie);

        if (request.Folio.HasValue)
            query = query.Where(d => d.CFOLIO == request.Folio.Value);

        if (request.ClienteProveedorId.HasValue)
            query = query.Where(d => d.CIDCLIENTEPROVEEDOR == request.ClienteProveedorId.Value);

        if (request.FechaDesde.HasValue)
            query = query.Where(d => d.CFECHA >= request.FechaDesde.Value);

        if (request.FechaHasta.HasValue)
            query = query.Where(d => d.CFECHA <= request.FechaHasta.Value);

        // Filtro por naturaleza: join con admConceptos
        if (request.Naturaleza.HasValue)
        {
            var conceptoIds = await _dbContext.Conceptos
                .Where(c => c.CNATURALEZA == request.Naturaleza.Value)
                .Select(c => c.CIDCONCEPTODOCUMENTO)
                .ToListAsync(cancellationToken);

            query = query.Where(d => conceptoIds.Contains(d.CIDCONCEPTODOCUMENTO));
        }

        // Filtro por código de concepto: join con admConceptos
        if (!string.IsNullOrWhiteSpace(request.CodigoConcepto))
        {
            var conceptoIds = await _dbContext.Conceptos
                .Where(c => c.CCODIGOCONCEPTO == request.CodigoConcepto)
                .Select(c => c.CIDCONCEPTODOCUMENTO)
                .ToListAsync(cancellationToken);

            query = query.Where(d => conceptoIds.Contains(d.CIDCONCEPTODOCUMENTO));
        }

        var take = request.Take is > 0 and <= 500 ? request.Take : 100;
        return await query.OrderByDescending(d => d.CFECHA).Take(take).ToListAsync(cancellationToken);
    }
}
