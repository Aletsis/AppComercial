using AppComercial.Api.Features.Almacenes;
using AppComercial.Api.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AppComercial.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlmacenesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AlmacenesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdmAlmacenes>>> Get([FromQuery] string? codigo)
    {
        try
        {
            var query = new GetAlmacenesQuery { Codigo = codigo };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener almacenes desde SQL Server: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<ActionResult<int>> Post([FromBody] CreateAlmacenCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al crear el almacén en el SDK: {ex.Message}");
        }
    }

    [HttpPut("{codigo}")]
    public async Task<ActionResult<int>> Put(string codigo, [FromBody] UpdateAlmacenCommand command)
    {
        try
        {
            command.Codigo = codigo;
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al actualizar el almacén en el SDK: {ex.Message}");
        }
    }
}

