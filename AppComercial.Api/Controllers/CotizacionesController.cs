using AppComercial.Api.Features.Cotizaciones;
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
    /// Lista las Cotizaciones. Se recomienda usar el filtro codigoConcepto para distinguirlas
    /// de otras naturaleza-1 (facturas, pedidos).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Models.AdmDocumentos>>> Get(
        [FromQuery] string? codigoConcepto,
        [FromQuery] string? serie,
        [FromQuery] int? clienteId,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] int take = 100)
    {
        try
        {
            var query = new GetCotizacionesQuery
            {
                CodigoConcepto = codigoConcepto,
                Serie          = serie,
                ClienteId      = clienteId,
                FechaDesde     = fechaDesde,
                FechaHasta     = fechaHasta,
                Take           = take
            };
            return Ok(await _mediator.Send(query));
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
            return Ok(await _mediator.Send(command));
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
        [FromBody] UpdateCotizacionCommand command)
    {
        try
        {
            command.CodigoConcepto = codigoConcepto;
            command.Serie          = serie;
            command.Folio          = folio;
            return Ok(await _mediator.Send(command));
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al actualizar la cotización en el SDK: {ex.Message}");
        }
    }
}
