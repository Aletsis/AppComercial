using AppComercial.Api.Features.SalidasAlmacen;
using AppComercial.Api.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AppComercial.Api.Controllers;

/// <summary>
/// Endpoints para Salidas de Almacén (Conceptos con CNATURALEZA = 5).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SalidasAlmacenController : ControllerBase
{
    private readonly IMediator _mediator;

    public SalidasAlmacenController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lista los documentos de Salida de Almacén, filtrados opcionalmente por concepto, serie o rango de fechas.
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
            var query = new GetSalidasAlmacenQuery
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
            return StatusCode(500, $"Error al obtener salidas de almacén: {ex.Message}");
        }
    }

    /// <summary>
    /// Crea una Salida de Almacén completa (cabecera + partidas) en un solo request.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<int>> Post([FromBody] CreateSalidaAlmacenCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al crear la salida de almacén en el SDK: {ex.Message}");
        }
    }

    /// <summary>
    /// Actualiza campos de un documento de Salida de Almacén existente.
    /// </summary>
    [HttpPut("{codigoConcepto}/{serie}/{folio}")]
    public async Task<ActionResult<int>> Put(
        string codigoConcepto, string serie, string folio, 
        [FromBody] Features.SalidasAlmacen.UpdateSalidaAlmacenCommand command)
    {
        try
        {
            command.CodigoConcepto = codigoConcepto;
            // Si serie viene como null o "%20" o "-", lo mandamos vacío al SDK.
            command.Serie          = string.IsNullOrWhiteSpace(serie) || serie == "-" || serie == "none" ? string.Empty : serie;
            command.Folio          = folio;
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al actualizar la salida de almacén en el SDK: {ex.Message}");
        }
    }
}
