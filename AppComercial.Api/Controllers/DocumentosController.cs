using AppComercial.Api.Features.Documentos;
using AppComercial.Api.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AppComercial.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentosController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdmDocumentos>>> Get([FromQuery] int? conceptoId, [FromQuery] string? serie, [FromQuery] double? folio, [FromQuery] int? clienteId)
    {
        try
        {
            var query = new GetDocumentosQuery 
            { 
                ConceptoDocumentoId = conceptoId, 
                Serie = serie, 
                Folio = folio, 
                ClienteProveedorId = clienteId 
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener documentos desde SQL Server: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<ActionResult<int>> Post([FromBody] CreateDocumentoCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al crear el documento en el SDK COM: {ex.Message}");
        }
    }

    [HttpPost("emitir")]
    public async Task<ActionResult<int>> Emitir([FromBody] EmitirDocumentoCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al emitir/timbrar el documento en el SDK COM: {ex.Message}");
        }
    }

    [HttpPost("cancelar")]
    public async Task<ActionResult<int>> Cancelar([FromBody] CancelarDocumentoCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al cancelar el documento en SDK COM: {ex.Message}");
        }
    }

    [HttpPost("saldar")]
    public async Task<ActionResult<int>> Saldar([FromBody] SaldarDocumentoCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al relacionar/saldar el documento en el SDK COM: {ex.Message}");
        }
    }

    [HttpPut("{codigoConcepto}/{serie}/{folio}")]
    public async Task<ActionResult<int>> Put(string codigoConcepto, string serie, string folio, [FromBody] UpdateDocumentoCommand command)
    {
        try
        {
            command.CodigoConcepto = codigoConcepto;
            command.Serie = serie;
            command.Folio = folio;
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al actualizar el documento en el SDK: {ex.Message}");
        }
    }
}

