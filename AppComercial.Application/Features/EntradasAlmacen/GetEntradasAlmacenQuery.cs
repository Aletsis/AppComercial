using MediatR;
using AppComercial.Domain.Entities;
using AppComercial.Domain.Interfaces;
using AppComercial.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Application.Features.EntradasAlmacen;

/// <summary>
/// Consulta Documentos cuyo Concepto tiene CIDDOCUMENTODE = 32 (Entradas de Almacén).
/// </summary>
public class GetEntradasAlmacenQuery : IRequest<IEnumerable<AdmDocumentos>>
{
    public string? CodigoConcepto { get; set; }
    public string? Serie { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
    public int? AlmacenId { get; set; }
}

public class GetEntradasAlmacenQueryHandler : IRequestHandler<GetEntradasAlmacenQuery, IEnumerable<AdmDocumentos>>
{
    private readonly IContpaqiDbContext _dbContext;

    public GetEntradasAlmacenQueryHandler(IContpaqiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<AdmDocumentos>> Handle(GetEntradasAlmacenQuery request, CancellationToken cancellationToken)
    {
        // Obtenemos IDs de conceptos que sean Entradas de Almacén (CIDDOCUMENTODE = 32)
        var conceptosEntrada = await _dbContext.Conceptos
            .Where(c => c.CIDDOCUMENTODE == 32)
            .Select(c => c.CIDCONCEPTODOCUMENTO)
            .ToListAsync(cancellationToken);

        var query = _dbContext.Documentos
            .AsNoTracking()
            .Where(d => conceptosEntrada.Contains(d.CIDCONCEPTODOCUMENTO));

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
