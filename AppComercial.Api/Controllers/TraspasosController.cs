using AppComercial.Application.Features.Traspasos;
using AppComercial.Domain.Entities;
using AppComercial.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AppComercial.Api.Controllers;

/// <summary>
/// Endpoints para Traspasos entre Almacenes (Conceptos con naturaleza Traspaso).
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TraspasosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TraspasosController> _logger;
    private readonly IContpaqiSdk _sdk;
    private readonly IConfiguration _config;

    public TraspasosController(IMediator mediator, ILogger<TraspasosController> logger, IContpaqiSdk sdk, IConfiguration config)
    {
        _mediator = mediator;
        _logger = logger;
        _sdk = sdk;
        _config = config;
    }

    /// <summary>
    /// Lista los documentos de Traspaso, filtrados opcionalmente por concepto, serie o rango de fechas.
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
            var query = new GetTraspasosQuery
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
            return StatusCode(500, $"Error al obtener traspasos: {ex.Message}");
        }
    }

    /// <summary>
    /// Crea un Traspaso entre almacenes completo (cabecera + partidas) en un solo request.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CreateTraspasoResult>> Post([FromBody] CreateTraspasoCommand command)
    {
        try
        {
            _logger.LogInformation("Recibida petición POST para Traspaso: {Payload}", 
                JsonSerializer.Serialize(command));

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            var innerMsg = ex.InnerException != null ? ex.InnerException.Message : "";
            _logger.LogError(ex, "Error en POST Traspasos");
            
            return StatusCode(500, new { 
                Message = "Error al crear el traspaso en CONTPAQi",
                Detalle = ex.Message,
                InnerException = innerMsg
            });
        }
    }
}
