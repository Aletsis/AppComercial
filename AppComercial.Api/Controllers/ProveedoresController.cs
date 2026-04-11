using AppComercial.Application.Features.Clientes;
using AppComercial.Application.DTOs;
using AppComercial.Domain.Entities;
using AppComercial.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AppComercial.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProveedoresController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProveedoresController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<ClienteDto>>> Get(
        [FromQuery] string? searchTerm,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            // En CONTPAQi Comercial el TipoCliente = 3 significa estrictamente Proveedor
            // (1 = Cliente, 2 = Cliente/Proveedor, 3 = Proveedor)
            var query = new GetClientesQuery 
            { 
                SearchTerm = searchTerm, 
                TipoCliente = 3,
                Page = page,
                PageSize = pageSize
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener proveedores desde SQL Server: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<ActionResult<int>> Post([FromBody] CreateClienteCommand command)
    {
        try
        {
            // Forzamos que sea un proveedor al momento de crearlo con nuestro comando
            command.TipoCliente = 3; 
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al crear el proveedor en el SDK COM: {ex.Message}");
        }
    }

    [HttpPut("{codigo}")]
    public async Task<ActionResult<int>> Put(string codigo, [FromBody] UpdateClienteCommand command)
    {
        try
        {
            // Forzamos que sea un proveedor al momento de actualizarlo
            command.TipoCliente = 3; 
            command.Codigo = codigo;
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al actualizar el proveedor en el SDK COM: {ex.Message}");
        }
    }
}
