using MediatR;
using AppComercial.Application.Common.Interfaces;
using AppComercial.Application.DTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AppComercial.Application.Features.Clientes;

public class GetClienteByCodigoQuery : IRequest<ClienteDetalleDto?>
{
    public string Codigo { get; set; } = string.Empty;

    public GetClienteByCodigoQuery() { }

    public GetClienteByCodigoQuery(string codigo)
    {
        Codigo = codigo;
    }
}

public class GetClienteByCodigoQueryHandler : IRequestHandler<GetClienteByCodigoQuery, ClienteDetalleDto?>
{
    private readonly IContpaqiDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetClienteByCodigoQueryHandler(IContpaqiDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<ClienteDetalleDto?> Handle(GetClienteByCodigoQuery request, CancellationToken cancellationToken)
    {
        var codigo = (request.Codigo ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(codigo))
            return null;

        var cliente = await _dbContext.Clientes.AsNoTracking()
            .FirstOrDefaultAsync(c => c.CCODIGOCLIENTE == codigo && (c.CTIPOCLIENTE == 1 || c.CTIPOCLIENTE == 2), cancellationToken);

        if (cliente == null)
            return null;

        var dto = _mapper.Map<ClienteDetalleDto>(cliente);

        // En CONTPAQi admDomicilios: TipoCatalogo = 1 es Cliente
        var domicilios = await _dbContext.Domicilios.AsNoTracking()
            .Where(d => d.CatalogoId == cliente.CIDCLIENTEPROVEEDOR && d.TipoCatalogo == 1)
            .ToListAsync(cancellationToken);

        var domFiscal = domicilios.FirstOrDefault(d => d.TipoDireccion == 0);
        if (domFiscal != null)
        {
            dto.DomicilioFiscal = _mapper.Map<DomicilioClienteDto>(domFiscal);
        }

        var domsEnvio = domicilios.Where(d => d.TipoDireccion == 1).ToList();
        if (domsEnvio.Count > 0)
        {
            dto.DomiciliosEnvio = _mapper.Map<List<DomicilioClienteDto>>(domsEnvio);
        }

        return dto;
    }
}
