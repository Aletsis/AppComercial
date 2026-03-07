using AppComercial.Api.Features.EntradasAlmacen;
using AppComercial.Api.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AppComercial.Api.Controllers;

/// <summary>
/// Endpoints para Entradas de Almacén (Conceptos con CNATURALEZA = 4).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EntradasAlmacenController : ControllerBase
{
    private readonly IMediator _mediator;

    public EntradasAlmacenController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lista los documentos de Entrada de Almacén, filtrados opcionalmente por concepto, serie o rango de fechas.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdmDocumentos>>> Get(
        [FromQuery] string? codigoConcepto,
        [FromQuery] string? serie,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta)
    {
        try
        {
            var query = new GetEntradasAlmacenQuery
            {
                CodigoConcepto = codigoConcepto,
                Serie          = serie,
                FechaDesde     = fechaDesde,
                FechaHasta     = fechaHasta
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener entradas de almacén: {ex.Message}");
        }
    }

    /// <summary>
    /// Crea una Entrada de Almacén completa (cabecera + partidas) en un solo request.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<int>> Post([FromBody] CreateEntradaAlmacenCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al crear la entrada de almacén en el SDK: {ex.Message}");
        }
    }

    /// <summary>
    /// Actualiza campos de un documento de Entrada de Almacén existente.
    /// Usa PUT /api/documentos/{concepto}/{serie}/{folio} para edición de datos del documento.
    /// </summary>
    [HttpPut("{codigoConcepto}/{serie}/{folio}")]
    public async Task<ActionResult<int>> Put(
        string codigoConcepto, string serie, string folio,
        [FromBody] Features.Documentos.UpdateDocumentoCommand command)
    {
        try
        {
            command.CodigoConcepto = codigoConcepto;
            command.Serie          = serie;
            command.Folio          = folio;
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al actualizar la entrada de almacén en el SDK: {ex.Message}");
        }
    }
}
