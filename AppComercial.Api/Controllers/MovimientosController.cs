using AppComercial.Api.Features.Movimientos;
using AppComercial.Api.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AppComercial.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MovimientosController : ControllerBase
{
    private readonly IMediator _mediator;

    public MovimientosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdmMovimientos>>> Get([FromQuery] int? documentoId, [FromQuery] int? productoId)
    {
        try
        {
            var query = new GetMovimientosQuery 
            { 
                DocumentoId = documentoId, 
                ProductoId = productoId 
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener movimientos desde SQL Server: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<ActionResult<int>> Post([FromBody] CreateMovimientoCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al crear el movimiento (partida) en el SDK COM: {ex.Message}");
        }
    }

    [HttpPut("{idMovimiento}")]
    public async Task<ActionResult<int>> Put(int idMovimiento, [FromBody] UpdateMovimientoCommand command)
    {
        try
        {
            command.IdMovimiento = idMovimiento;
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al actualizar el movimiento en el SDK: {ex.Message}");
        }
    }
}

