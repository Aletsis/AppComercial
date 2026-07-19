using MediatR;
using AppComercial.Domain.Entities;
using AppComercial.Domain.Interfaces;
using AppComercial.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Application.Features.Compras;

/// <summary>
/// Consulta Documentos cuyo Concepto tiene CNATURALEZA = 2 (Compras).
/// </summary>
public class GetComprasQuery : IRequest<IEnumerable<AdmDocumentos>>
{
    public string? CodigoConcepto { get; set; }
    public string? Serie { get; set; }
    public double? Folio { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public int? ProveedorId { get; set; }
}

public class GetComprasQueryHandler : IRequestHandler<GetComprasQuery, IEnumerable<AdmDocumentos>>
{
    private readonly IContpaqiDbContext _dbContext;

    public GetComprasQueryHandler(IContpaqiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<AdmDocumentos>> Handle(GetComprasQuery request, CancellationToken cancellationToken)
    {
        // CIDDOCUMENTODE = 19 → Compras
        var conceptosCompra = await _dbContext.Conceptos
            .Where(c => c.CIDDOCUMENTODE == 19)
            .Select(c => c.CIDCONCEPTODOCUMENTO)
            .ToListAsync(cancellationToken);

        var query = _dbContext.Documentos
            .AsNoTracking()
            .Where(d => conceptosCompra.Contains(d.CIDCONCEPTODOCUMENTO));

        if (!string.IsNullOrEmpty(request.CodigoConcepto))
        {
            // CCODIGOCONCEPTO en la tabla puede tener trailing spaces
            var requestedCode = request.CodigoConcepto.Trim();
            var idConcepto = await _dbContext.Conceptos
                .Where(c => c.CCODIGOCONCEPTO != null && c.CCODIGOCONCEPTO.Trim() == requestedCode)
                .Select(c => c.CIDCONCEPTODOCUMENTO)
                .FirstOrDefaultAsync(cancellationToken);
                
            query = query.Where(d => d.CIDCONCEPTODOCUMENTO == idConcepto);
        }

        if (!string.IsNullOrEmpty(request.Serie))
            query = query.Where(d => d.CSERIEDOCUMENTO == request.Serie);

        if (request.Folio.HasValue)
            query = query.Where(d => d.CFOLIO == request.Folio.Value);

        if (request.FechaDesde.HasValue)
            query = query.Where(d => d.CFECHA >= request.FechaDesde.Value);

        if (request.FechaHasta.HasValue)
            query = query.Where(d => d.CFECHA <= request.FechaHasta.Value);

        if (request.ProveedorId.HasValue)
            query = query.Where(d => d.CIDCLIENTEPROVEEDOR == request.ProveedorId.Value);

        var rawResults = await (from d in query
                                 join c in _dbContext.Conceptos.AsNoTracking() on d.CIDCONCEPTODOCUMENTO equals c.CIDCONCEPTODOCUMENTO
                                 select new { d, ConceptoCodigo = c.CCODIGOCONCEPTO, c.CIDALMASUM })
                                .OrderByDescending(x => x.d.CIDDOCUMENTO)
                                .Take(100)
                                .ToListAsync(cancellationToken);

        foreach (var item in rawResults)
        {
            item.d.CCODIGOCONCEPTO = item.ConceptoCodigo;
            item.d.CIDALMACEN = item.CIDALMASUM;
        }

        return rawResults.Select(x => x.d);
    }
}
