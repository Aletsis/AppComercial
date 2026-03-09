using AppComercial.Api.Features.Compras;
using AppComercial.Api.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AppComercial.Api.Controllers;

/// <summary>
/// Endpoints para Compras (Conceptos con CNATURALEZA = 2).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ComprasController : ControllerBase
{
    private readonly IMediator _mediator;

    public ComprasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lista los documentos de Compra, filtrados opcionalmente por concepto, proveedor, serie o rango de fechas.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdmDocumentos>>> Get(
        [FromQuery] string? codigoConcepto,
        [FromQuery] string? serie,
        [FromQuery] int? proveedorId,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta)
    {
        try
        {
            var query = new GetComprasQuery
            {
                CodigoConcepto = codigoConcepto,
                Serie          = serie,
                ProveedorId    = proveedorId,
                FechaDesde     = fechaDesde,
                FechaHasta     = fechaHasta
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener compras: {ex.Message}");
        }
    }

    /// <summary>
    /// Crea un documento de Compra completo (cabecera + partidas) en un solo request.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<int>> Post([FromBody] CreateCompraCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al crear la compra en el SDK: {ex.Message}");
        }
    }

    /// <summary>
    /// Actualiza campos de un documento de Compra existente.
    /// </summary>
    [HttpPut("{codigoConcepto}/{serie}/{folio}")]
    public async Task<ActionResult<int>> Put(
        string codigoConcepto, string serie, string folio,
        [FromBody] UpdateCompraCommand command)
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
            return StatusCode(500, $"Error al actualizar la compra en el SDK: {ex.Message}");
        }
    }
}
