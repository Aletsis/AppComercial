using AppComercial.Application.Features.SalidasAlmacen;
using AppComercial.Domain.Entities;
using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Interfaces.SdkModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AppComercial.Api.Controllers;

/// <summary>
/// Endpoints para Salidas de Almacén (Conceptos con CNATURALEZA = 5).
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SalidasAlmacenController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SalidasAlmacenController> _logger;
    private readonly IContpaqiSdk _sdk;
    private readonly IConfiguration _config;

    public SalidasAlmacenController(IMediator mediator, ILogger<SalidasAlmacenController> logger, IContpaqiSdk sdk, IConfiguration config)
    {
        _mediator = mediator;
        _logger = logger;
        _sdk = sdk;
        _config = config;
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
    public async Task<ActionResult<CreateSalidaAlmacenResult>> Post([FromBody] CreateSalidaAlmacenCommand command)
    {
        try
        {
            _logger.LogInformation("Recibida petición POST para Salida Almacén: {Payload}", 
                JsonSerializer.Serialize(command));

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            var innerMsg = ex.InnerException != null ? ex.InnerException.Message : "";
            var errorDetails = $"Error al crear la salida de almacén. Ex: {ex.Message} | Inner: {innerMsg} | Trace: {ex.StackTrace}";
            _logger.LogError(ex, "Error en POST SalidasAlmacen");
            
            return StatusCode(500, new { 
                Message = "Error interno del servidor",
                Detalle = ex.Message,
                InnerException = innerMsg,
                StackTrace = ex.StackTrace 
            });
        }
    }

    /// <summary>
    /// Actualiza campos de un documento de Salida de Almacén existente.
    /// </summary>
    [HttpPut("{codigoConcepto}/{serie}/{folio}")]
    public async Task<ActionResult<int>> Put(
        string codigoConcepto, string serie, string folio, 
        [FromBody] AppComercial.Application.Features.SalidasAlmacen.UpdateSalidaAlmacenCommand command)
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

    /// <summary>
    /// Genera y descarga el PDF de una Salida de Almacén usando la forma impresa configurada en CONTPAQi.
    /// Usar "-" como serie si el documento no tiene serie.
    /// </summary>
    [HttpGet("{codigoConcepto}/{serie}/{folio}/pdf")]
    public async Task<IActionResult> GetPdf(string codigoConcepto, string serie, double folio)
    {
        try
        {
            var rutaEmpresa = _config["Contpaqi:DirectorioEmpresa"] 
                ?? throw new InvalidOperationException("Contpaqi:DirectorioEmpresa no configurado.");

            // Serie "-" o "none" la mapeamos a vacío (convenio de URL para series vacías)
            if (serie == "-" || serie == "none") serie = string.Empty;

            _logger.LogInformation("Generando PDF para Salida: {Concepto}/{Serie}/{Folio}", codigoConcepto, serie, folio);

            string rutaPdf = await _sdk.GenerarPdfAsync(codigoConcepto, serie, folio, rutaEmpresa);

            var bytes = await System.IO.File.ReadAllBytesAsync(rutaPdf);
            return File(bytes, "application/pdf", $"{codigoConcepto}{serie}{folio}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar PDF para Salida {Concepto}/{Serie}/{Folio}", codigoConcepto, serie, folio);
            return StatusCode(500, new { Message = "Error al generar el PDF", Detalle = ex.Message });
        }
    }
}
