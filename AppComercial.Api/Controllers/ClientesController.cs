using AppComercial.Application.Features.Clientes;
using AppComercial.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

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

    /// <summary>
    /// Consulta paginada del catálogo de clientes con filtros opcionales.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<ClienteDto>>> Get(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool onlyActive = true)
    {
        try
        {
            var query = new GetClientesQuery 
            { 
                SearchTerm = search, 
                TipoCliente = 1, // 1 = Clientes y Mixtos
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

    /// <summary>
    /// Obtiene la ficha detallada de un cliente por su código, incluyendo su domicilio fiscal y condiciones comerciales.
    /// </summary>
    [HttpGet("{codigo}")]
    public async Task<ActionResult<ClienteDetalleDto>> GetByCodigo(string codigo)
    {
        try
        {
            var query = new GetClienteByCodigoQuery(codigo);
            var result = await _mediator.Send(query);
            if (result == null)
                return NotFound($"No se encontró ningún cliente con el código '{codigo}'.");

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener detalle del cliente: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene el saldo actual, saldo vencido, crédito disponible y documentos pendientes del cliente.
    /// </summary>
    [HttpGet("{codigo}/saldo")]
    public async Task<ActionResult<SaldoClienteDto>> GetSaldo(string codigo)
    {
        try
        {
            var query = new GetSaldoClienteQuery(codigo);
            var result = await _mediator.Send(query);
            if (result == null)
                return NotFound($"No se encontró ningún cliente con el código '{codigo}'.");

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener saldo del cliente: {ex.Message}");
        }
    }

    /// <summary>
    /// Registra un nuevo cliente en CONTPAQi Comercial vía SDK con opción de domicilio fiscal.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<int>> Post([FromBody] CreateClienteCommand command)
    {
        try
        {
            command.TipoCliente = 1;
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetByCodigo), new { codigo = command.Codigo }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al crear el cliente en el SDK: {ex.Message}");
        }
    }

    /// <summary>
    /// Actualiza los datos generales, fiscales, de crédito o domicilio de un cliente existente.
    /// </summary>
    [HttpPut("{codigo}")]
    public async Task<ActionResult<int>> Put(string codigo, [FromBody] UpdateClienteCommand command)
    {
        try
        {
            command.TipoCliente = 1;
            command.Codigo = codigo;
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al actualizar el cliente en el SDK: {ex.Message}");
        }
    }

    /// <summary>
    /// Modifica el estatus de un cliente (Baja lógica o reactivación).
    /// </summary>
    [HttpPatch("{codigo}/estatus")]
    public async Task<ActionResult<bool>> CambiarEstatus(string codigo, [FromQuery] bool activo)
    {
        try
        {
            var command = new CambiarEstatusClienteCommand(codigo, activo);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al actualizar el estatus del cliente: {ex.Message}");
        }
    }
}
