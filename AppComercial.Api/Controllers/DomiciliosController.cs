using AppComercial.Api.Features.Domicilios;
using AppComercial.Api.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AppComercial.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DomiciliosController : ControllerBase
{
    private readonly IMediator _mediator;

    public DomiciliosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdmDomicilios>>> Get([FromQuery] int? catalogoId, [FromQuery] int? tipoCatalogo)
    {
        try
        {
            var query = new GetDomiciliosQuery { CatalogoId = catalogoId, TipoCatalogo = tipoCatalogo };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener domicilios desde SQL Server: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<ActionResult<int>> Post([FromBody] CreateDomicilioCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al crear el domicilio en el SDK COM: {ex.Message}");
        }
    }

    [HttpPut("{idDireccion}")]
    public async Task<ActionResult<int>> Put(int idDireccion, [FromBody] UpdateDomicilioCommand command)
    {
        try
        {
            command.IdDireccion = idDireccion;
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al actualizar el domicilio en el SDK: {ex.Message}");
        }
    }
}

