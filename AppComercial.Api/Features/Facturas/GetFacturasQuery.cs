using MediatR;
using AppComercial.Api.Models;
using AppComercial.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Api.Features.Facturas;

/// <summary>
/// Consulta para obtener Facturas de Venta desde SQL Server.
/// Filtra automáticamente por Conceptos con CNATURALEZA = 1 (Venta).
/// </summary>
public class GetFacturasQuery : IRequest<IEnumerable<AdmDocumentos>>
{
    /// <summary>Filtra por código de concepto. Ejemplo: "FAC", "FACT".</summary>
    public string? CodigoConcepto { get; set; }

    /// <summary>Filtra por serie. Ejemplo: "A".</summary>
    public string? Serie { get; set; }

    /// <summary>Filtra por ID interno del cliente.</summary>
    public int? ClienteId { get; set; }

    /// <summary>Filtra documentos a partir de esta fecha (inclusive).</summary>
    public DateTime? FechaDesde { get; set; }

    /// <summary>Filtra documentos hasta esta fecha (inclusive).</summary>
    public DateTime? FechaHasta { get; set; }

    /// <summary>Máximo de registros a retornar. Por defecto: 100. Máximo permitido: 500.</summary>
    public int Take { get; set; } = 100;
}

public class GetFacturasQueryHandler : IRequestHandler<GetFacturasQuery, IEnumerable<AdmDocumentos>>
{
    private readonly ContpaqiDbContext _dbContext;

    public GetFacturasQueryHandler(ContpaqiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<AdmDocumentos>> Handle(GetFacturasQuery request, CancellationToken cancellationToken)
    {
        // Obtener los IDs de conceptos con naturaleza Venta (1)
        var conceptosVentaIds = _dbContext.Conceptos
            .Where(c => c.CNATURALEZA == 1);

        if (!string.IsNullOrWhiteSpace(request.CodigoConcepto))
            conceptosVentaIds = conceptosVentaIds.Where(c => c.CCODIGOCONCEPTO == request.CodigoConcepto);

        var ids = await conceptosVentaIds
            .Select(c => c.CIDCONCEPTODOCUMENTO)
            .ToListAsync(cancellationToken);

        var query = _dbContext.Documentos
            .AsNoTracking()
            .Where(d => ids.Contains(d.CIDCONCEPTODOCUMENTO));

        if (!string.IsNullOrWhiteSpace(request.Serie))
            query = query.Where(d => d.CSERIEDOCUMENTO == request.Serie);

        if (request.ClienteId.HasValue)
            query = query.Where(d => d.CIDCLIENTEPROVEEDOR == request.ClienteId.Value);

        if (request.FechaDesde.HasValue)
            query = query.Where(d => d.CFECHA >= request.FechaDesde.Value);

        if (request.FechaHasta.HasValue)
            query = query.Where(d => d.CFECHA <= request.FechaHasta.Value);

        var take = request.Take is > 0 and <= 500 ? request.Take : 100;
        return await query.OrderByDescending(d => d.CFECHA).Take(take).ToListAsync(cancellationToken);
    }
}
