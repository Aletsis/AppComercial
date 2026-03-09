using AppComercial.Api.Features.Cotizaciones;
using AppComercial.Api.Features.Documentos;
using AppComercial.Api.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AppComercial.Api.Controllers;

/// <summary>
/// Endpoints para Cotizaciones (Conceptos con CNATURALEZA = 1, de tipo cotización).
/// Las cotizaciones NO afectan inventario.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CotizacionesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CotizacionesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lista los documentos de Cotización, filtrados opcionalmente por concepto, cliente, serie o rango de fechas.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdmDocumentos>>> Get(
        [FromQuery] string? codigoConcepto,
        [FromQuery] string? serie,
        [FromQuery] int? clienteId,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta)
    {
        try
        {
            var query = new GetDocumentosQuery
            {
                CodigoConcepto     = codigoConcepto,
                Serie              = serie,
                ClienteProveedorId = clienteId,
                FechaDesde         = fechaDesde,
                FechaHasta         = fechaHasta,
                Naturaleza         = 1 // Ventas (incluye cotizaciones según el concepto configurado)
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener cotizaciones: {ex.Message}");
        }
    }

    /// <summary>
    /// Crea una Cotización completa (cabecera + partidas) en un solo request.
    /// No afecta existencias de inventario.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<int>> Post([FromBody] CreateCotizacionCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al crear la cotización en el SDK: {ex.Message}");
        }
    }

    /// <summary>
    /// Actualiza campos de una Cotización existente.
    /// </summary>
    [HttpPut("{codigoConcepto}/{serie}/{folio}")]
    public async Task<ActionResult<int>> Put(
        string codigoConcepto, string serie, string folio,
        [FromBody] UpdateDocumentoCommand command)
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
            return StatusCode(500, $"Error al actualizar la cotización en el SDK: {ex.Message}");
        }
    }
}
