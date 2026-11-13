using MediatR;
using AppComercial.Application.Common.Interfaces;
using AppComercial.Application.DTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AppComercial.Application.Features.Proveedores;

public class GetProveedorByCodigoQuery : IRequest<ProveedorDetalleDto?>
{
    public string Codigo { get; set; } = string.Empty;

    public GetProveedorByCodigoQuery() { }

    public GetProveedorByCodigoQuery(string codigo)
    {
        Codigo = codigo;
    }
}

public class GetProveedorByCodigoQueryHandler : IRequestHandler<GetProveedorByCodigoQuery, ProveedorDetalleDto?>
{
    private readonly IContpaqiDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetProveedorByCodigoQueryHandler(IContpaqiDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<ProveedorDetalleDto?> Handle(GetProveedorByCodigoQuery request, CancellationToken cancellationToken)
    {
        var codigo = (request.Codigo ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(codigo))
            return null;

        var proveedor = await _dbContext.Clientes.AsNoTracking()
            .FirstOrDefaultAsync(c => c.CCODIGOCLIENTE == codigo && (c.CTIPOCLIENTE == 3 || c.CTIPOCLIENTE == 2), cancellationToken);

        if (proveedor == null)
            return null;

        var dto = _mapper.Map<ProveedorDetalleDto>(proveedor);

        // En CONTPAQi admDomicilios: TipoCatalogo = 2 es Proveedor (también verificamos 1 en caso de registros compartidos)
        var domicilios = await _dbContext.Domicilios.AsNoTracking()
            .Where(d => d.CatalogoId == proveedor.CIDCLIENTEPROVEEDOR && (d.TipoCatalogo == 2 || d.TipoCatalogo == 1))
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
