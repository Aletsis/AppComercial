using MediatR;
using AppComercial.Api.Models;
using AppComercial.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Api.Features.Cotizaciones;

/// <summary>
/// Consulta para obtener Cotizaciones desde SQL Server.
/// Filtra por Conceptos con CNATURALEZA = 1 (Venta) que correspondan al código indicado.
/// Para distinguir cotizaciones de facturas o pedidos, use el filtro CodigoConcepto.
/// </summary>
public class GetCotizacionesQuery : IRequest<IEnumerable<AdmDocumentos>>
{
    /// <summary>
    /// Filtra por código de concepto de cotización. Recomendado para distinguir de facturas.
    /// Ejemplo: "COT", "COTI"
    /// </summary>
    public string? CodigoConcepto { get; set; }

    /// <summary>Filtra por serie.</summary>
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

public class GetCotizacionesQueryHandler : IRequestHandler<GetCotizacionesQuery, IEnumerable<AdmDocumentos>>
{
    private readonly ContpaqiDbContext _dbContext;

    public GetCotizacionesQueryHandler(ContpaqiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<AdmDocumentos>> Handle(GetCotizacionesQuery request, CancellationToken cancellationToken)
    {
        // Filtrar conceptos de naturaleza Venta (1)
        var conceptosQuery = _dbContext.Conceptos.Where(c => c.CNATURALEZA == 1);

        if (!string.IsNullOrWhiteSpace(request.CodigoConcepto))
            conceptosQuery = conceptosQuery.Where(c => c.CCODIGOCONCEPTO == request.CodigoConcepto);

        var ids = await conceptosQuery
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
