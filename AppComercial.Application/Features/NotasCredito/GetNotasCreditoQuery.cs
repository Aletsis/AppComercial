using MediatR;
using AppComercial.Domain.Entities;
using AppComercial.Domain.Interfaces;
using AppComercial.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AppComercial.Application.Features.NotasCredito;

/// <summary>
/// Consulta para obtener Notas de Crédito y Devoluciones sobre Venta desde SQL Server.
/// Filtra automáticamente por Conceptos con CNATURALEZA = 3 (Devolución sobre Venta).
/// </summary>
public class GetNotasCreditoQuery : IRequest<IEnumerable<AdmDocumentos>>
{
    /// <summary>Filtra por código de concepto. Ejemplo: "DEV", "NC", "DEVVTA".</summary>
    public string? CodigoConcepto { get; set; }

    /// <summary>Filtra por serie.</summary>
    public string? Serie { get; set; }

    /// <summary>Filtra por ID interno del cliente que realizó la devolución.</summary>
    public int? ClienteId { get; set; }

    /// <summary>Filtra documentos a partir de esta fecha (inclusive).</summary>
    public DateTime? FechaDesde { get; set; }

    /// <summary>Filtra documentos hasta esta fecha (inclusive).</summary>
    public DateTime? FechaHasta { get; set; }

    /// <summary>Máximo de registros a retornar. Por defecto: 100. Máximo permitido: 500.</summary>
    public int Take { get; set; } = 100;
}

public class GetNotasCreditoQueryHandler : IRequestHandler<GetNotasCreditoQuery, IEnumerable<AdmDocumentos>>
{
    private readonly IContpaqiDbContext _dbContext;

    public GetNotasCreditoQueryHandler(IContpaqiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<AdmDocumentos>> Handle(GetNotasCreditoQuery request, CancellationToken cancellationToken)
    {
        // Filtrar conceptos de naturaleza Devolución sobre Venta (3)
        var conceptosQuery = _dbContext.Conceptos.Where(c => c.CNATURALEZA == 3);

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
