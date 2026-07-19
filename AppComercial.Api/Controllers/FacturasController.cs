using AppComercial.Application.Features.Facturas;
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
    /// Lista las Facturas de Venta, filtradas opcionalmente por concepto, cliente, serie o rango de fechas.
    /// Filtra automáticamente por Conceptos con CNATURALEZA = 1.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppComercial.Domain.Entities.AdmDocumentos>>> Get(
        [FromQuery] string? codigoConcepto,
        [FromQuery] string? serie,
        [FromQuery] int? clienteId,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] int take = 100)
    {
        try
        {
            var query = new GetFacturasQuery
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
            return StatusCode(500, $"Error al obtener facturas: {ex.Message}");
        }
    }

    /// <summary>
    /// Crea una Factura de Cliente (cabecera + partidas). 
    /// Puede timbrar automáticamente si command.AutoTimbrar = true.
    /// </summary>
    [HttpPost("generar")]
    public async Task<ActionResult<CreateFacturaResult>> Generar([FromBody] CreateFacturaCommand command)
    {
        try
        {
            return Ok(await _mediator.Send(command));
        }
        catch (Exception ex)
        {
            // TODO: Inyectar ILogger para registro estructurado
            return StatusCode(500, $"Error al generar la factura en el SDK: {ex.Message}");
        }
    }

    /// <summary>
    /// Crea una Factura CFDI completa (legacy endpoint).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CreateFacturaResult>> Post([FromBody] CreateFacturaCommand command) => await Generar(command);

    /// <summary>
    /// Crea una Factura Global CFDI 4.0 para el Público en General.
    /// </summary>
    [HttpPost("global")]
    public async Task<ActionResult<CreateFacturaResult>> PostGlobal([FromBody] CreateFacturaGlobalCommand command)
    {
        try
        {
            return Ok(await _mediator.Send(command));
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al crear la factura global: {ex.Message}");
        }
    }

    /// <summary>
    /// Actualiza campos de una Factura existente.
    /// </summary>
    [HttpPut("{codigoConcepto}/{serie}/{folio}")]
    public async Task<ActionResult<int>> Put(
        string codigoConcepto, string serie, string folio,
        [FromBody] UpdateFacturaCommand command)
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
            return StatusCode(500, $"Error al actualizar la factura en el SDK: {ex.Message}");
        }
    }

    /// <summary>
    /// Timbra una Factura existente como CFDI. Devuelve datos fiscales detallados (UUID, Sellos, etc.).
    /// </summary>
    [HttpPost("timbrar")]
    public async Task<ActionResult<AppComercial.Application.DTOs.TimbradoResult>> Timbrar([FromBody] AppComercial.Application.Features.Documentos.EmitirDocumentoCommand command)
    {
        try
        {
            return Ok(await _mediator.Send(command));
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al timbrar la factura: {ex.Message}");
        }
    }

    /// <summary>
    /// Emite/timbra una Factura (legacy endpoint).
    /// </summary>
    [HttpPost("emitir")]
    public async Task<ActionResult<AppComercial.Application.DTOs.TimbradoResult>> Emitir([FromBody] AppComercial.Application.Features.Documentos.EmitirDocumentoCommand command) => await Timbrar(command);

    /// <summary>
    /// Cancela una Factura existente.
    /// </summary>
    [HttpPost("cancelar")]
    public async Task<ActionResult<int>> Cancelar([FromBody] AppComercial.Application.Features.Documentos.CancelarDocumentoCommand command)
    {
        try
        {
            return Ok(await _mediator.Send(command));
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
    public async Task<ActionResult<int>> Saldar([FromBody] AppComercial.Application.Features.Documentos.SaldarDocumentoCommand command)
    {
        try
        {
            return Ok(await _mediator.Send(command));
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al saldar la factura: {ex.Message}");
        }
    }
}
