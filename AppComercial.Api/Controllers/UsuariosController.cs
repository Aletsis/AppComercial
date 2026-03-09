using AppComercial.Api.Features.Usuarios;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AppComercial.Api.Controllers;

/// <summary>
/// Endpoints para consultar Usuarios de CONTPAQi Comercial.
/// 
/// NOTA TÉCNICA: CONTPAQi Comercial no almacena sus usuarios en la base de datos
/// de la empresa (adEMPRESA_*). Los usuarios se gestionan internamente desde la
/// aplicación CONTPAQi. Lo disponible vía SQL es la tabla UsuariosActivos en
/// CompacWAdmin, que muestra qué usuarios tienen sesión abierta en este momento.
/// 
/// Para crear o eliminar usuarios, utilice la interfaz de administración de CONTPAQi.
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
    /// Lista los usuarios con sesión activa en CONTPAQi Comercial en este momento.
    /// Filtra opcionalmente por código de usuario o empresa.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsuarioActivoDto>>> Get(
        [FromQuery] string? codigoUsuario,
        [FromQuery] string? empresa)
    {
        try
        {
            var query = new GetUsuariosQuery
            {
                CodigoUsuario = codigoUsuario,
                Empresa       = empresa
            };
            return Ok(await _mediator.Send(query));
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener usuarios activos: {ex.Message}");
        }
    }
}
