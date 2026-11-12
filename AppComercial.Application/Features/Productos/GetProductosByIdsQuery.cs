using MediatR;
using AppComercial.Application.DTOs;
using AppComercial.Application.Common.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AppComercial.Application.Features.Productos;

public class GetProductosByIdsQuery : IRequest<IEnumerable<ProductoDto>>
{
    public List<int> Ids { get; set; } = new();

    public GetProductosByIdsQuery() { }

    public GetProductosByIdsQuery(IEnumerable<int> ids)
    {
        Ids = ids?.Distinct().ToList() ?? new List<int>();
    }
}

public class GetProductosByIdsQueryHandler : IRequestHandler<GetProductosByIdsQuery, IEnumerable<ProductoDto>>
{
    private readonly IContpaqiDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetProductosByIdsQueryHandler(IContpaqiDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductoDto>> Handle(GetProductosByIdsQuery request, CancellationToken cancellationToken)
    {
        if (request.Ids == null || request.Ids.Count == 0)
            return Enumerable.Empty<ProductoDto>();

        var entities = await _dbContext.Productos
            .AsNoTracking()
            .Where(p => request.Ids.Contains(p.CIDPRODUCTO))
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<ProductoDto>>(entities);
    }
}
