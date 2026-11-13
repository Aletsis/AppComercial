using AppComercial.Application.Features.Proveedores;
using AppComercial.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

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

    /// <summary>
    /// Consulta paginada del catálogo de proveedores con filtros opcionales.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<ProveedorDto>>> Get(
        [FromQuery] string? searchTerm,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] bool onlyActive = true)
    {
        try
        {
            var query = new GetProveedoresQuery 
            { 
                SearchTerm = searchTerm, 
                Page = page,
                PageSize = pageSize,
                OnlyActive = onlyActive
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener proveedores: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene la ficha detallada de un proveedor por su código, incluyendo domicilio y condiciones de compra.
    /// </summary>
    [HttpGet("{codigo}")]
    public async Task<ActionResult<ProveedorDetalleDto>> GetByCodigo(string codigo)
    {
        try
        {
            var query = new GetProveedorByCodigoQuery(codigo);
            var result = await _mediator.Send(query);
            if (result == null)
                return NotFound($"No se encontró ningún proveedor con el código '{codigo}'.");

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener detalle del proveedor: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene el saldo actual pendiente de pago, saldo vencido y documentos pendientes de un proveedor.
    /// </summary>
    [HttpGet("{codigo}/saldo")]
    public async Task<ActionResult<SaldoProveedorDto>> GetSaldo(string codigo)
    {
        try
        {
            var query = new GetSaldoProveedorQuery(codigo);
            var result = await _mediator.Send(query);
            if (result == null)
                return NotFound($"No se encontró ningún proveedor con el código '{codigo}'.");

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener saldo del proveedor: {ex.Message}");
        }
    }

    /// <summary>
    /// Registra un nuevo proveedor en CONTPAQi Comercial vía SDK con opción de domicilio fiscal.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<int>> Post([FromBody] CreateProveedorCommand command)
    {
        try
        {
            if (command.TipoCliente != 2) command.TipoCliente = 3;
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetByCodigo), new { codigo = command.Codigo }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al crear el proveedor en el SDK: {ex.Message}");
        }
    }

    /// <summary>
    /// Actualiza los datos generales, fiscales, de crédito o compra de un proveedor existente.
    /// </summary>
    [HttpPut("{codigo}")]
    public async Task<ActionResult<int>> Put(string codigo, [FromBody] UpdateProveedorCommand command)
    {
        try
        {
            if (command.TipoCliente.HasValue && command.TipoCliente.Value != 2) command.TipoCliente = 3;
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
            return StatusCode(500, $"Error al actualizar el proveedor en el SDK: {ex.Message}");
        }
    }

    /// <summary>
    /// Modifica el estatus de un proveedor (Baja lógica o reactivación).
    /// </summary>
    [HttpPatch("{codigo}/estatus")]
    public async Task<ActionResult<bool>> CambiarEstatus(string codigo, [FromQuery] bool activo)
    {
        try
        {
            var command = new CambiarEstatusProveedorCommand(codigo, activo);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al actualizar el estatus del proveedor: {ex.Message}");
        }
    }
}
