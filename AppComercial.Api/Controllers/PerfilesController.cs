using AppComercial.Api.Features.Perfiles;
using AppComercial.Api.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AppComercial.Api.Controllers;

/// <summary>
/// Endpoints para consultar los Perfiles de usuario de CONTPAQi Comercial.
/// Los perfiles definen los permisos y accesos de cada usuario.
/// La creación y edición de perfiles se realiza desde el sistema CONTPAQi directamente.
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
    /// Lista los perfiles disponibles en el sistema, filtrados opcionalmente por código o estatus.
    /// Use el campo Id del perfil para asignarlo al crear o actualizar un usuario.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdmPerfiles>>> Get(
        [FromQuery] string? codigoPerfil,
        [FromQuery] int? estatus)
    {
        try
        {
            var query = new GetPerfilesQuery
            {
                CodigoPerfil = codigoPerfil,
                Estatus      = estatus ?? 0
            };
            return Ok(await _mediator.Send(query));
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener perfiles: {ex.Message}");
        }
    }
}
