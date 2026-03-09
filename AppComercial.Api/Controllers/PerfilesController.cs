using AppComercial.Api.Features.Perfiles;
using AppComercial.Api.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AppComercial.Api.Controllers;

/// <summary>
/// Endpoints para gestionar Perfiles de permisos de CONTPAQi Comercial.
/// Los perfiles se almacenan en RepositorioAdminPAQ.dbo.CAC30000.
/// La configuración detallada de permisos por módulo se gestiona desde la aplicación CONTPAQi.
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
    /// Lista los perfiles de permisos disponibles en CONTPAQi Comercial.
    /// Use el IdPerfil para asignarlo al crear o actualizar un usuario.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CacPerfil>>> Get(
        [FromQuery] string? descripcion)
    {
        try
        {
            var query = new GetPerfilesQuery { Descripcion = descripcion };
            return Ok(await _mediator.Send(query));
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener perfiles: {ex.Message}");
        }
    }
}
