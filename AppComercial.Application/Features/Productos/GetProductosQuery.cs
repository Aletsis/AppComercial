using MediatR;
using AppComercial.Domain.Entities;
using AppComercial.Domain.Interfaces;
using AppComercial.Application.DTOs;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AppComercial.Application.Features.Productos;

public class GetProductosQuery : IRequest<PaginatedResult<ProductoDto>>
{
    public string? SearchTerm { get; set; }
    public int? TipoProducto { get; set; }
    public bool OnlyActive { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class GetProductosQueryHandler : IRequestHandler<GetProductosQuery, PaginatedResult<ProductoDto>>
{
    private readonly IProductoRepository _productoRepository;
    private readonly IMapper _mapper;

    public GetProductosQueryHandler(IProductoRepository productoRepository, IMapper mapper)
    {
        _productoRepository = productoRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<ProductoDto>> Handle(GetProductosQuery request, CancellationToken cancellationToken)
    {
        // NOTA: Para verdadera EFICIENCIA, el repositorio debería devolver IQueryable.
        // Como el repositorio actual devuelve IEnumerable/List, lo hacemos en memoria por ahora,
        // pero marcamos este punto para futura optimización en el repositorio.
        
        IEnumerable<AdmProductos> entities;
        
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            entities = await _productoRepository.GetByNombreAsync(request.SearchTerm);
        }
        else
        {
            entities = await _productoRepository.GetByFiltersAsync(null, request.TipoProducto);
        }

        if (request.OnlyActive)
        {
            entities = entities.Where(e => e.CSTATUSPRODUCTO == 1).ToList();
        }

        var totalItems = entities.Count();
        var pagedEntities = entities
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var dtos = _mapper.Map<IEnumerable<ProductoDto>>(pagedEntities);

        return new PaginatedResult<ProductoDto>
        {
            Items = dtos,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize)
        };
    }
}
