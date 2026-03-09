using AppComercial.Api.Features.Perfiles;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AppComercial.Api.Controllers;

/// <summary>
/// Endpoints para consultar las Empresas registradas en el sistema CONTPAQi Comercial.
/// 
/// NOTA: Los perfiles de usuario en CONTPAQi no se almacenan en tablas SQL accesibles.
/// Se gestionan internamente desde la aplicación CONTPAQi Comercial.
/// Este controller expone en cambio las Empresas disponibles en el sistema,
/// información útil para saber a qué empresa conectar el SDK.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PerfilesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PerfilesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lista las empresas registradas en CONTPAQi Comercial.
    /// Use la Ruta de la empresa para configurar DirectorioEmpresa en appsettings.json.
    /// </summary>
    [HttpGet("empresas")]
    public async Task<ActionResult<IEnumerable<EmpresaDto>>> GetEmpresas(
        [FromQuery] string? nombre)
    {
        try
        {
            var query = new GetEmpresasQuery { Nombre = nombre };
            return Ok(await _mediator.Send(query));
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener empresas: {ex.Message}");
        }
    }
}
