using MediatR;
using AppComercial.Application.Common.Interfaces;
using AppComercial.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AppComercial.Application.Features.Clientes;

public class GetSaldoClienteQuery : IRequest<SaldoClienteDto?>
{
    public string Codigo { get; set; } = string.Empty;

    public GetSaldoClienteQuery() { }

    public GetSaldoClienteQuery(string codigo)
    {
        Codigo = codigo;
    }
}

public class GetSaldoClienteQueryHandler : IRequestHandler<GetSaldoClienteQuery, SaldoClienteDto?>
{
    private readonly IContpaqiDbContext _context;

    public GetSaldoClienteQueryHandler(IContpaqiDbContext context)
    {
        _context = context;
    }

    public async Task<SaldoClienteDto?> Handle(GetSaldoClienteQuery request, CancellationToken cancellationToken)
    {
        var codigo = (request.Codigo ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(codigo))
            return null;

        var cliente = await _context.Clientes.AsNoTracking()
            .FirstOrDefaultAsync(c => c.CCODIGOCLIENTE == codigo, cancellationToken);

        if (cliente == null)
            return null;

        var hoy = DateTime.Today;

        // Documentos de venta con saldo pendiente
        var docs = await _context.Documentos.AsNoTracking()
            .Where(d => d.CIDCLIENTEPROVEEDOR == cliente.CIDCLIENTEPROVEEDOR &&
                        d.CCANCELADO == 0 &&
                        d.CPENDIENTE > 0.001 &&
                        d.CUSACLIENTE == 1)
            .OrderBy(d => d.CFECHAVENCIMIENTO ?? d.CFECHA)
            .ToListAsync(cancellationToken);

        var saldoActual = docs.Sum(d => d.CPENDIENTE);
        var docsVencidos = docs.Where(d => d.CFECHAVENCIMIENTO.HasValue && d.CFECHAVENCIMIENTO.Value.Date < hoy).ToList();
        var saldoVencido = docsVencidos.Sum(d => d.CPENDIENTE);
        var limiteCredito = cliente.CLIMITECREDITOCLIENTE;
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

        return new SaldoClienteDto
        {
            ClienteId = cliente.CIDCLIENTEPROVEEDOR,
            Codigo = cliente.CCODIGOCLIENTE?.Trim() ?? string.Empty,
            RazonSocial = cliente.CRAZONSOCIAL?.Trim() ?? string.Empty,
            RFC = cliente.CRFC?.Trim(),
            LimiteCredito = limiteCredito,
            DiasCredito = cliente.CDIASCREDITOCLIENTE,
            SaldoActual = Math.Round(saldoActual, 2),
            SaldoVencido = Math.Round(saldoVencido, 2),
            CreditoDisponible = Math.Round(creditoDisponible, 2),
            TotalDocumentosPendientes = docs.Count,
            TotalDocumentosVencidos = docsVencidos.Count,
            DocumentosPendientes = listadoDocs
        };
    }
}
