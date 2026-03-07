using AppComercial.Api.Features.Monedas;
using AppComercial.Api.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AppComercial.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MonedasController : ControllerBase
{
    private readonly IMediator _mediator;

    public MonedasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdmMonedas>>> Get()
    {
        try
        {
            var query = new GetMonedasQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener monedas desde SQL Server: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<ActionResult<int>> Post([FromBody] CreateMonedaCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al crear la moneda en el SDK: {ex.Message}");
        }
    }

    [HttpPut("{idMoneda}")]
    public async Task<ActionResult<int>> Put(int idMoneda, [FromBody] UpdateMonedaCommand command)
    {
        try
        {
            command.IdMoneda = idMoneda;
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al actualizar la moneda en el SDK: {ex.Message}");
        }
    }
}

