using MediatR;
using AppComercial.Domain.Entities;
using AppComercial.Domain.Interfaces;
using AppComercial.Application.Common.Interfaces;
using AppComercial.Application.DTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AppComercial.Application.Features.Clientes;

public class GetClientesQuery : IRequest<PaginatedResult<ClienteDto>>
{
    public string? SearchTerm { get; set; }
    public int? TipoCliente { get; set; }
    public bool OnlyActive { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class GetClientesQueryHandler : IRequestHandler<GetClientesQuery, PaginatedResult<ClienteDto>>
{
    private readonly IContpaqiDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetClientesQueryHandler(IContpaqiDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<ClienteDto>> Handle(GetClientesQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Clientes.AsNoTracking();

        if (request.OnlyActive)
        {
            query = query.Where(c => c.CESTATUS == 1);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(c => (c.CCODIGOCLIENTE != null && c.CCODIGOCLIENTE.Contains(request.SearchTerm)) || (c.CRAZONSOCIAL != null && c.CRAZONSOCIAL.Contains(request.SearchTerm)));
        }

        if (request.TipoCliente.HasValue)
        {
            if (request.TipoCliente.Value == 1) // Clientes + Mixtos
                query = query.Where(c => c.CTIPOCLIENTE == 1 || c.CTIPOCLIENTE == 2);
            else if (request.TipoCliente.Value == 3) // Proveedores + Mixtos
                query = query.Where(c => c.CTIPOCLIENTE == 3 || c.CTIPOCLIENTE == 2);
            else
                query = query.Where(c => c.CTIPOCLIENTE == request.TipoCliente.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(c => c.CRAZONSOCIAL)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<IEnumerable<ClienteDto>>(items);

        return new PaginatedResult<ClienteDto>
        {
            Items = dtos,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize)
        };
    }
}
