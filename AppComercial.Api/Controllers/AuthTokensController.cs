using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AppComercial.Application.Features.Auth;

namespace AppComercial.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/auth")]
public class AuthTokensController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthTokensController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Endpoint central para iniciar sesión con un usuario de CONTPAQi.
    /// Valida credenciales contra la tabla CAC10000 de RepositorioAdminPAQ y devuelve un Token JWT.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Ocurrió un error en el motor de autenticación.", details = ex.Message });
        }
    }
}
