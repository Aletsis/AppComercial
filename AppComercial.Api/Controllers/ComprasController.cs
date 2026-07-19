using AppComercial.Application.Features.Compras;
using AppComercial.Domain.Entities;
using AppComercial.Domain.Interfaces;
using AppComercial.Domain.Interfaces.SdkModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppComercial.Api.Controllers;

/// <summary>
/// Endpoints para Compras (Conceptos con CNATURALEZA = 2).
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ComprasController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ComprasController> _logger;
    private readonly IContpaqiSdk _sdk;
    private readonly IConfiguration _config;

    public ComprasController(IMediator mediator, ILogger<ComprasController> logger, IContpaqiSdk sdk, IConfiguration config)
    {
        _mediator = mediator;
        _logger = logger;
        _sdk = sdk;
        _config = config;
    }

    /// <summary>
    /// Lista los documentos de Compra, filtrados opcionalmente por concepto, proveedor, serie o rango de fechas.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdmDocumentos>>> Get(
        [FromQuery] string? codigoConcepto,
        [FromQuery] string? serie,
        [FromQuery] double? folio,
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
                Folio          = folio,
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

    /// <summary>
    /// Genera y descarga el PDF de una Compra usando la forma impresa configurada en CONTPAQi.
    /// Usar "-" como serie si el documento no tiene serie.
    /// </summary>
    [HttpGet("{codigoConcepto}/{serie}/{folio}/pdf")]
    public async Task<IActionResult> GetPdf(string codigoConcepto, string serie, double folio)
    {
        try
        {
            var rutaEmpresa = _config["Contpaqi:DirectorioEmpresa"]
                ?? throw new InvalidOperationException("Contpaqi:DirectorioEmpresa no configurado.");

            if (serie == "-" || serie == "none") serie = string.Empty;

            _logger.LogInformation("Generando PDF para Compra: {Concepto}/{Serie}/{Folio}", codigoConcepto, serie, folio);

            string rutaPdf = await _sdk.GenerarPdfAsync(codigoConcepto, serie, folio, rutaEmpresa);

            var bytes = await System.IO.File.ReadAllBytesAsync(rutaPdf);
            return File(bytes, "application/pdf", $"{codigoConcepto}{serie}{folio}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar PDF para Compra {Concepto}/{Serie}/{Folio}", codigoConcepto, serie, folio);
            return StatusCode(500, new { Message = "Error al generar el PDF", Detalle = ex.Message });
        }
    }
}
