using MediatR;
using AppComercial.Api.Models;
using AppComercial.Api.Infrastructure.Repositories;

namespace AppComercial.Api.Features.Productos;

public class GetProductosQuery : IRequest<IEnumerable<AdmProductos>>
{
    public string? Codigo { get; set; }
    public int? TipoProducto { get; set; } // Opcional: Para Filtrar Productos(1), Paquetes(2), Servicios(3)
}

public class GetProductosQueryHandler : IRequestHandler<GetProductosQuery, IEnumerable<AdmProductos>>
{
    private readonly IProductoRepository _productoRepository;

    public GetProductosQueryHandler(IProductoRepository productoRepository)
    {
        _productoRepository = productoRepository;
    }

    public async Task<IEnumerable<AdmProductos>> Handle(GetProductosQuery request, CancellationToken cancellationToken)
    {
        return await _productoRepository.GetByFiltersAsync(request.Codigo, request.TipoProducto);
    }
}
