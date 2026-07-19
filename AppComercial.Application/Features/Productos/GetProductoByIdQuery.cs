using MediatR;
using AppComercial.Domain.Interfaces;
using AppComercial.Application.DTOs;
using AutoMapper;
using System.Threading;
using System.Threading.Tasks;

namespace AppComercial.Application.Features.Productos;

public record GetProductoByIdQuery(int Id) : IRequest<ProductoDto?>;

public class GetProductoByIdQueryHandler : IRequestHandler<GetProductoByIdQuery, ProductoDto?>
{
    private readonly IProductoRepository _productoRepository;
    private readonly IMapper _mapper;

    public GetProductoByIdQueryHandler(IProductoRepository productoRepository, IMapper mapper)
    {
        _productoRepository = productoRepository;
        _mapper = mapper;
    }

    public async Task<ProductoDto?> Handle(GetProductoByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _productoRepository.GetByIdAsync(request.Id);
        if (entity == null) return null;
        return _mapper.Map<ProductoDto>(entity);
    }
}
