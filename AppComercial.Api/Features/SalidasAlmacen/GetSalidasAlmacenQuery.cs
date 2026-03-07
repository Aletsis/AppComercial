using MediatR;
using AppComercial.Api.Models;
using AppComercial.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Api.Features.SalidasAlmacen;

/// <summary>
/// Consulta Documentos cuyo Concepto tiene CNATURALEZA = 5 (Salidas de Almacén).
/// </summary>
public class GetSalidasAlmacenQuery : IRequest<IEnumerable<AdmDocumentos>>
{
    public string? CodigoConcepto { get; set; }
    public string? Serie { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
}

public class GetSalidasAlmacenQueryHandler : IRequestHandler<GetSalidasAlmacenQuery, IEnumerable<AdmDocumentos>>
{
    private readonly ContpaqiDbContext _dbContext;

    public GetSalidasAlmacenQueryHandler(ContpaqiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<AdmDocumentos>> Handle(GetSalidasAlmacenQuery request, CancellationToken cancellationToken)
    {
        // CNATURALEZA = 5 → Salidas de Almacén
        var conceptosSalida = await _dbContext.Conceptos
            .Where(c => c.CNATURALEZA == 5)
            .Select(c => c.CIDCONCEPTODOCUMENTO)
            .ToListAsync(cancellationToken);

        var query = _dbContext.Documentos
            .AsNoTracking()
            .Where(d => conceptosSalida.Contains(d.CIDCONCEPTODOCUMENTO));

        if (!string.IsNullOrEmpty(request.CodigoConcepto))
        {
            var idConcepto = await _dbContext.Conceptos
                .Where(c => c.CCODIGOCONCEPTO == request.CodigoConcepto)
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

        return await query.OrderByDescending(d => d.CFECHA).Take(100).ToListAsync(cancellationToken);
    }
}
