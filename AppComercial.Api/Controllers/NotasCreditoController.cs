using AppComercial.Api.Features.NotasCredito;
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
    /// Lista las Notas de Crédito y Devoluciones sobre Venta.
    /// Filtra automáticamente por Conceptos con CNATURALEZA = 3.
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
            var query = new GetNotasCreditoQuery
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
            return StatusCode(500, $"Error al obtener notas de crédito: {ex.Message}");
        }
    }

    /// <summary>
    /// Crea una Nota de Crédito completa (cabecera + partidas) en un solo request.
    /// Los productos regresan al almacén especificado en cada partida.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<int>> Post([FromBody] CreateNotaCreditoCommand command)
    {
        try
        {
            return Ok(await _mediator.Send(command));
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
        [FromBody] UpdateNotaCreditoCommand command)
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
            return StatusCode(500, $"Error al actualizar la nota de crédito en el SDK: {ex.Message}");
        }
    }

    /// <summary>
    /// Emite/timbra una Nota de Crédito como CFDI (cuando aplica).
    /// </summary>
    [HttpPost("emitir")]
    public async Task<ActionResult<int>> Emitir([FromBody] Features.Documentos.EmitirDocumentoCommand command)
    {
        try
        {
            return Ok(await _mediator.Send(command));
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
    public async Task<ActionResult<int>> Cancelar([FromBody] Features.Documentos.CancelarDocumentoCommand command)
    {
        try
        {
            return Ok(await _mediator.Send(command));
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al cancelar la nota de crédito: {ex.Message}");
        }
    }
}
