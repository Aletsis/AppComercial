using Microsoft.AspNetCore.Mvc;
using AppComercial.Api.Application.DTOs;
using AppComercial.Api.Infrastructure.Repositories;
using AutoMapper;

namespace AppComercial.Api.Bff.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/bff/productos")]
public class ProductosBffController : ControllerBase
{
    private readonly IProductoRepository _productoRepository;
    private readonly IMapper _mapper;

    public ProductosBffController(IProductoRepository productoRepository, IMapper mapper)
    {
        _productoRepository = productoRepository;
        _mapper = mapper;
    }

    [HttpGet("search")]
    public async Task<ActionResult<PaginatedResult<ProductoDto>>> Search(
        [FromQuery] string? q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var query = string.IsNullOrWhiteSpace(q)
            ? await _productoRepository.GetAllAsync()
            : await _productoRepository.GetByNombreAsync(q);

        var totalItems = query.Count();
        var items = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var dtos = _mapper.Map<IEnumerable<ProductoDto>>(items);

        var result = new PaginatedResult<ProductoDto>
        {
            Items = dtos,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };

        return Ok(result);
    }
}