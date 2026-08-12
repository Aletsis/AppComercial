using MediatR;
using AppComercial.Domain.Entities;
using AppComercial.Domain.Interfaces;
using AppComercial.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Application.Features.Traspasos;

/// <summary>
/// Consulta Documentos cuyo Concepto tiene CIDDOCUMENTODE = 34 (Traspasos entre almacenes) o naturaleza Traspaso.
/// </summary>
public class GetTraspasosQuery : IRequest<IEnumerable<AdmDocumentos>>
{
    public string? CodigoConcepto { get; set; }
    public string? Serie { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
}

public class GetTraspasosQueryHandler : IRequestHandler<GetTraspasosQuery, IEnumerable<AdmDocumentos>>
{
    private readonly IContpaqiDbContext _dbContext;

    public GetTraspasosQueryHandler(IContpaqiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<AdmDocumentos>> Handle(GetTraspasosQuery request, CancellationToken cancellationToken)
    {
        // CIDDOCUMENTODE = 34 o CNATURALEZA = 6 → Traspasos
        var conceptosTraspaso = await _dbContext.Conceptos
            .Where(c => c.CIDDOCUMENTODE == 34 || c.CNATURALEZA == 6)
            .Select(c => c.CIDCONCEPTODOCUMENTO)
            .ToListAsync(cancellationToken);

        var query = _dbContext.Documentos
            .AsNoTracking()
            .Where(d => conceptosTraspaso.Contains(d.CIDCONCEPTODOCUMENTO));

        if (!string.IsNullOrEmpty(request.CodigoConcepto))
        {
            var requestedCode = request.CodigoConcepto.Trim();
            var idConcepto = await _dbContext.Conceptos
                .Where(c => c.CCODIGOCONCEPTO != null && c.CCODIGOCONCEPTO.Trim() == requestedCode)
                .Select(c => c.CIDCONCEPTODOCUMENTO)
                .FirstOrDefaultAsync(cancellationToken);

            query = query.Where(d => d.CIDCONCEPTODOCUMENTO == idConcepto);
        }

        if (!string.IsNullOrEmpty(request.Serie))
            query = query.Where(d => d.CSERIEDOCUMENTO == request.Serie);

        if (request.FechaDesde.HasValue)
            query = query.Where(d => d.CFECHA >= request.FechaDesde.Value);

        if (request.FechaHasta.HasValue)
            query = query.Where(d => d.CFECHA <= request.FechaHasta.Value);

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
