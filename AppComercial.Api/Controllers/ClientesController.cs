using AppComercial.Application.Features.Clientes;
using AppComercial.Application.DTOs;
using AppComercial.Domain.Entities;
using AppComercial.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AppComercial.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClientesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<ClienteDto>>> Get(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool onlyActive = true)
    {
        try
        {
            // Para el endpoint de Clientes, exigimos Tipo=1 (Cliente)
            var query = new GetClientesQuery 
            { 
                SearchTerm = search, 
                TipoCliente = 1,
                Page = page,
                PageSize = pageSize,
                OnlyActive = onlyActive
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener clientes: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<ActionResult<int>> Post([FromBody] CreateClienteCommand command)
    {
        try
        {
            // Forzamos que sea un cliente
            command.TipoCliente = 1;
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al crear el cliente en el SDK COM: {ex.Message}");
        }
    }

    [HttpPut("{codigo}")]
    public async Task<ActionResult<int>> Put(string codigo, [FromBody] UpdateClienteCommand command)
    {
        try
        {
            // Forzamos que sea un cliente
            command.TipoCliente = 1;
            command.Codigo = codigo;
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al actualizar el cliente en el SDK COM: {ex.Message}");
        }
    }
}
