using MediatR;
using AppComercial.Domain.Entities;
using AppComercial.Application.Common.Interfaces;
using AppComercial.Application.DTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AppComercial.Application.Features.Proveedores;

public class GetProveedoresQuery : IRequest<PaginatedResult<ProveedorDto>>
{
    public string? SearchTerm { get; set; }
    public bool OnlyActive { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class GetProveedoresQueryHandler : IRequestHandler<GetProveedoresQuery, PaginatedResult<ProveedorDto>>
{
    private readonly IContpaqiDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetProveedoresQueryHandler(IContpaqiDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<ProveedorDto>> Handle(GetProveedoresQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Clientes.AsNoTracking();

        // En CONTPAQi admClientes: CTIPOCLIENTE 3 = Proveedor, 2 = Cliente/Proveedor (Mixto)
        query = query.Where(c => c.CTIPOCLIENTE == 3 || c.CTIPOCLIENTE == 2);

        if (request.OnlyActive)
        {
            query = query.Where(c => c.CESTATUS == 1);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(c => 
                (c.CCODIGOCLIENTE != null && c.CCODIGOCLIENTE.Contains(term)) || 
                (c.CRAZONSOCIAL != null && c.CRAZONSOCIAL.Contains(term)) ||
                (c.CRFC != null && c.CRFC.Contains(term)) ||
                (c.CDENCOMERCIAL != null && c.CDENCOMERCIAL.Contains(term)));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(c => c.CRAZONSOCIAL)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<IEnumerable<ProveedorDto>>(items);

        return new PaginatedResult<ProveedorDto>
        {
            Items = dtos,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize)
        };
    }
}
