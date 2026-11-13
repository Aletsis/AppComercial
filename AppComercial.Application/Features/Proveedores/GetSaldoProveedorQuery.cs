using MediatR;
using AppComercial.Application.Common.Interfaces;
using AppComercial.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AppComercial.Application.Features.Proveedores;

public class GetSaldoProveedorQuery : IRequest<SaldoProveedorDto?>
{
    public string Codigo { get; set; } = string.Empty;

    public GetSaldoProveedorQuery() { }

    public GetSaldoProveedorQuery(string codigo)
    {
        Codigo = codigo;
    }
}

public class GetSaldoProveedorQueryHandler : IRequestHandler<GetSaldoProveedorQuery, SaldoProveedorDto?>
{
    private readonly IContpaqiDbContext _context;

    public GetSaldoProveedorQueryHandler(IContpaqiDbContext context)
    {
        _context = context;
    }

    public async Task<SaldoProveedorDto?> Handle(GetSaldoProveedorQuery request, CancellationToken cancellationToken)
    {
        var codigo = (request.Codigo ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(codigo))
            return null;

        var proveedor = await _context.Clientes.AsNoTracking()
            .FirstOrDefaultAsync(c => c.CCODIGOCLIENTE == codigo, cancellationToken);

        if (proveedor == null)
            return null;

        var hoy = DateTime.Today;

        // Documentos de compras o pasivos con saldo pendiente hacia el proveedor
        var docs = await _context.Documentos.AsNoTracking()
            .Where(d => d.CIDCLIENTEPROVEEDOR == proveedor.CIDCLIENTEPROVEEDOR &&
                        d.CCANCELADO == 0 &&
                        d.CPENDIENTE > 0.001 &&
                        d.CUSAPROVEEDOR == 1)
            .OrderBy(d => d.CFECHAVENCIMIENTO ?? d.CFECHA)
            .ToListAsync(cancellationToken);

        var saldoActual = docs.Sum(d => d.CPENDIENTE);
        var docsVencidos = docs.Where(d => d.CFECHAVENCIMIENTO.HasValue && d.CFECHAVENCIMIENTO.Value.Date < hoy).ToList();
        var saldoVencido = docsVencidos.Sum(d => d.CPENDIENTE);
        var limiteCredito = proveedor.CLIMITECREDITOPROVEEDOR;
        var creditoDisponible = Math.Max(0, limiteCredito - saldoActual);

        var listadoDocs = docs.Select(d =>
        {
            int diasVencido = 0;
            if (d.CFECHAVENCIMIENTO.HasValue && d.CFECHAVENCIMIENTO.Value.Date < hoy)
            {
                diasVencido = (int)(hoy - d.CFECHAVENCIMIENTO.Value.Date).TotalDays;
            }

            return new DocumentoSaldoDto
            {
                DocumentoId = d.CIDDOCUMENTO,
                Serie = d.CSERIEDOCUMENTO?.Trim() ?? string.Empty,
                Folio = d.CFOLIO,
                Fecha = d.CFECHA,
                FechaVencimiento = d.CFECHAVENCIMIENTO,
                Total = d.CTOTAL,
                SaldoPendiente = d.CPENDIENTE,
                DiasVencido = diasVencido
            };
        }).ToList();

        return new SaldoProveedorDto
        {
            ProveedorId = proveedor.CIDCLIENTEPROVEEDOR,
            Codigo = proveedor.CCODIGOCLIENTE?.Trim() ?? string.Empty,
            RazonSocial = proveedor.CRAZONSOCIAL?.Trim() ?? string.Empty,
            RFC = proveedor.CRFC?.Trim(),
            LimiteCredito = limiteCredito,
            DiasCredito = proveedor.CDIASCREDITOPROVEEDOR,
            SaldoActual = Math.Round(saldoActual, 2),
            SaldoVencido = Math.Round(saldoVencido, 2),
            CreditoDisponible = Math.Round(creditoDisponible, 2),
            TotalDocumentosPendientes = docs.Count,
            TotalDocumentosVencidos = docsVencidos.Count,
            DocumentosPendientes = listadoDocs
        };
    }
}
