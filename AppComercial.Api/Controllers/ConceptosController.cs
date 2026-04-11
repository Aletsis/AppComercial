using AppComercial.Application.Features.Conceptos;
using AppComercial.Domain.Entities;
using AppComercial.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AppComercial.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConceptosController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConceptosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdmConceptos>>> Get([FromQuery] string? codigo, [FromQuery] int? tipoDocumento)
    {
        try
        {
            var query = new GetConceptosQuery { Codigo = codigo, TipoDocumento = tipoDocumento };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener conceptos desde SQL Server: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<ActionResult<int>> Post([FromBody] CreateConceptoCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al crear el concepto en el SDK: {ex.Message}");
        }
    }

    [HttpPut("{codigo}")]
    public async Task<ActionResult<int>> Put(string codigo, [FromBody] UpdateConceptoCommand command)
    {
        try
        {
            command.Codigo = codigo;
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al actualizar el concepto en el SDK: {ex.Message}");
        }
    }
}

