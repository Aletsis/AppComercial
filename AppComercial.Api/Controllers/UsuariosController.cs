using AppComercial.Api.Features.Usuarios;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AppComercial.Api.Controllers;

/// <summary>
/// Endpoints para gestionar Usuarios de CONTPAQi Comercial.
/// La lectura/escritura se realiza directamente sobre la tabla admUsuarios.
/// IMPORTANTE: Las contraseñas nunca se retornan en respuestas GET.
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
    /// Lista los usuarios del sistema, filtrados opcionalmente por código, perfil o estatus.
    /// Las contraseñas se excluyen de la respuesta por seguridad.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsuarioDto>>> Get(
        [FromQuery] string? codigoUsuario,
        [FromQuery] int? idPerfil,
        [FromQuery] int? estatus)
    {
        try
        {
            var query = new GetUsuariosQuery
            {
                CodigoUsuario = codigoUsuario,
                IdPerfil      = idPerfil,
                Estatus       = estatus ?? 0
            };
            return Ok(await _mediator.Send(query));
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener usuarios: {ex.Message}");
        }
    }

    /// <summary>
    /// Crea un nuevo usuario en CONTPAQi Comercial.
    /// El usuario quedará activo (Estatus=0) por defecto.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<int>> Post([FromBody] CreateUsuarioCommand command)
    {
        try
        {
            var id = await _mediator.Send(command);
            return Ok(id);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al crear el usuario: {ex.Message}");
        }
    }

    /// <summary>
    /// Actualiza datos de un usuario existente.
    /// El código de usuario (login) no es editable para evitar inconsistencias.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<bool>> Put(int id, [FromBody] UpdateUsuarioCommand command)
    {
        try
        {
            command.Id = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al actualizar el usuario: {ex.Message}");
        }
    }
}
