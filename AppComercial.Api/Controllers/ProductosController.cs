using AppComercial.Application.Features.Productos;
using AppComercial.Application.DTOs;
using AppComercial.Domain.Entities;
using AppComercial.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AppComercial.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<ProductoDto>>> Get(
        [FromQuery] string? search, 
        [FromQuery] int? tipo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool onlyActive = true)
    {
        try
        {
            var query = new GetProductosQuery 
            { 
                SearchTerm = search, 
                TipoProducto = tipo,
                Page = page,
                PageSize = pageSize,
                OnlyActive = onlyActive
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener productos: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<ActionResult<int>> Post([FromBody] CreateProductoCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al crear el producto en el SDK COM: {ex.Message}");
        }
    }

    [HttpPut("{codigo}")]
    public async Task<ActionResult<int>> Put(string codigo, [FromBody] UpdateProductoCommand command)
    {
        try
        {
            command.Codigo = codigo;
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al actualizar el producto en el SDK COM: {ex.Message}");
        }
    }
}
