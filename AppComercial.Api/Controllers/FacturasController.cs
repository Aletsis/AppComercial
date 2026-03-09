using AppComercial.Api.Features.Facturas;
using AppComercial.Api.Features.Documentos;
using AppComercial.Api.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AppComercial.Api.Controllers;

/// <summary>
/// Endpoints para Facturas de Venta (Conceptos con CNATURALEZA = 1).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FacturasController : ControllerBase
{
    private readonly IMediator _mediator;

    public FacturasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lista los documentos de Factura, filtrados opcionalmente por concepto, cliente, serie o rango de fechas.
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
                CodigoConcepto      = codigoConcepto,
                Serie               = serie,
                ClienteProveedorId  = clienteId,
                FechaDesde          = fechaDesde,
                FechaHasta          = fechaHasta,
                Naturaleza          = 1 // Ventas
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener facturas: {ex.Message}");
        }
    }

    /// <summary>
    /// Crea una Factura de Venta completa (cabecera + partidas) en un solo request.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<int>> Post([FromBody] CreateFacturaCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al crear la factura en el SDK: {ex.Message}");
        }
    }

    /// <summary>
    /// Actualiza campos de una Factura existente.
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
            return StatusCode(500, $"Error al actualizar la factura en el SDK: {ex.Message}");
        }
    }

    /// <summary>
    /// Emite/timbra una Factura como CFDI.
    /// </summary>
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
            return StatusCode(500, $"Error al timbrar la factura: {ex.Message}");
        }
    }

    /// <summary>
    /// Cancela una Factura existente.
    /// </summary>
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
            return StatusCode(500, $"Error al cancelar la factura: {ex.Message}");
        }
    }

    /// <summary>
    /// Salda una Factura contra un documento de pago.
    /// </summary>
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
            return StatusCode(500, $"Error al saldar la factura: {ex.Message}");
        }
    }
}
