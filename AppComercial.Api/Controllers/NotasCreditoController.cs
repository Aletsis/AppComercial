using AppComercial.Api.Features.NotasCredito;
using AppComercial.Api.Features.Documentos;
using AppComercial.Api.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AppComercial.Api.Controllers;

/// <summary>
/// Endpoints para Notas de Crédito y Devoluciones sobre Venta (Conceptos con CNATURALEZA = 3).
/// Estas operaciones revierten parcial o totalmente una venta y regresan mercancía al inventario.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class NotasCreditoController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotasCreditoController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lista los documentos de Nota de Crédito, filtrados opcionalmente por concepto, cliente, serie o rango de fechas.
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
                Naturaleza         = 3 // Devoluciones sobre venta
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener notas de crédito: {ex.Message}");
        }
    }

    /// <summary>
    /// Crea una Nota de Crédito / Devolución completa (cabecera + partidas) en un solo request.
    /// Los productos regresan al almacén especificado en cada partida.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<int>> Post([FromBody] CreateNotaCreditoCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al crear la nota de crédito en el SDK: {ex.Message}");
        }
    }

    /// <summary>
    /// Actualiza campos de una Nota de Crédito existente.
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
            return StatusCode(500, $"Error al actualizar la nota de crédito en el SDK: {ex.Message}");
        }
    }

    /// <summary>
    /// Emite/timbra una Nota de Crédito como CFDI (cuando aplica).
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
            return StatusCode(500, $"Error al timbrar la nota de crédito: {ex.Message}");
        }
    }

    /// <summary>
    /// Cancela una Nota de Crédito existente.
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
            return StatusCode(500, $"Error al cancelar la nota de crédito: {ex.Message}");
        }
    }
}
