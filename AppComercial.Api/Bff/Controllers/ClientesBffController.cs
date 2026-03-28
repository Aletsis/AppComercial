using Microsoft.AspNetCore.Mvc;
using AppComercial.Api.Application.DTOs;
using AppComercial.Api.Infrastructure.Repositories;
using AutoMapper;

namespace AppComercial.Api.Bff.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/bff/clientes")]
public class ClientesBffController : ControllerBase
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IMapper _mapper;

    public ClientesBffController(IClienteRepository clienteRepository, IMapper mapper)
    {
        _clienteRepository = clienteRepository;
        _mapper = mapper;
    }

    [HttpGet("search")]
    public async Task<ActionResult<PaginatedResult<ClienteDto>>> Search(
        [FromQuery] string? q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var query = string.IsNullOrWhiteSpace(q)
            ? await _clienteRepository.GetAllAsync()
            : await _clienteRepository.GetByNombreAsync(q);

        var totalItems = query.Count();
        var items = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var dtos = _mapper.Map<IEnumerable<ClienteDto>>(items);

        var result = new PaginatedResult<ClienteDto>
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