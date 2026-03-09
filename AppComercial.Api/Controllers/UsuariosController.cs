using AppComercial.Api.Features.Usuarios;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AppComercial.Api.Controllers;

/// <summary>
/// Endpoints para gestionar Usuarios de CONTPAQi Comercial.
/// Los usuarios se almacenan en RepositorioAdminPAQ.dbo.CAC10000.
/// Las contraseñas nunca se retornan en respuestas GET.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsuariosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lista los usuarios de CONTPAQi Comercial (IDSISTEMA=5).
    /// Filtros opcionales: clave (login), nombre, idPerfil.
    /// Las contraseñas se excluyen de la respuesta por seguridad.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsuarioDto>>> Get(
        [FromQuery] string? clave,
        [FromQuery] string? nombre,
        [FromQuery] int? idPerfil)
    {
        try
        {
            var query = new GetUsuariosQuery
            {
                Clave    = clave,
                Nombre   = nombre,
                IdPerfil = idPerfil
            };
            return Ok(await _mediator.Send(query));
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener usuarios: {ex.Message}");
        }
    }
}
