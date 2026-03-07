using AppComercial.Api.Features.UnidadesMedida;
using AppComercial.Api.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AppComercial.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UnidadesMedidaController : ControllerBase
{
    private readonly IMediator _mediator;

    public UnidadesMedidaController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdmUnidadesMedidaPeso>>> Get([FromQuery] string? nombre)
    {
        try
        {
            var query = new GetUnidadesMedidaQuery { Nombre = nombre };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener unidades de medida desde SQL Server: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<ActionResult<int>> Post([FromBody] CreateUnidadMedidaCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al crear unidad de medida en el SDK COM: {ex.Message}");
        }
    }

    [HttpPut("{nombre}")]
    public async Task<ActionResult<int>> Put(string nombre, [FromBody] UpdateUnidadMedidaCommand command)
    {
        try
        {
            command.NombreUnidad = nombre;
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al actualizar la unidad de medida en el SDK: {ex.Message}");
        }
    }
}

